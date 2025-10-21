/* =========================================================================
   ORS (Obstacle Report System) – site.js
   - Login-gate med kart i bakgrunnen
   - Rolig, kontinuerlig drift før innlogging (konfigurerbar)
   - Fade-out av overlay
   - Robust login: fungerer selv uten posisjonstilgang
   - Ved Login: "snap" tilbake til geoHome (første geo) ellers fallback
   ======================================================================= */

(function (window, document) {
    "use strict";

    // ------- CONFIG ----------------------------------------------------------
    const CONFIG = {
        DRIFT_AMPLITUDE_PX: 1,         // 1–4 anbefales
        DRIFT_ANGULAR_VELOCITY: 0.01,  // rad/s (0.01–0.04 = rolig)
        DRIFT_SMOOTHING: 0.12,         // 0–1; høyere = mer dempet
        OVERLAY_FADE_MS: 280,          // ms
        GEO_ZOOM: 14,
        FALLBACK_CENTER: [63.4305, 10.3951],
        FALLBACK_ZOOM: 6,
        FLY_DURATION: 0.2,             // sek – pen snap tilbake ved login
        MOVEEND_FAILSAFE_MS: 1200,     // ms – sikkerhetsnett i tilfelle moveend ikke fyrer
        CENTER_EPSILON_DEG: 1e-5       // terskel for å anse kartet "allerede der"
    };
    // ------------------------------------------------------------------------

    const ORS = (window.ORS = window.ORS || {});

    /* ----------------------------- Utils ---------------------------------- */
    function assertLeaflet() {
        if (!window.L) {
            console.warn("[ORS] Leaflet (L) ikke funnet. Last Leaflet CSS/JS i _Layout.cshtml før site.js.");
            return false;
        }
        return true;
    }

    function debounce(fn, wait) {
        let t;
        return function (...args) {
            clearTimeout(t);
            t = setTimeout(() => fn.apply(this, args), wait);
        };
    }

    function fadeOut(el, durationMs) {
        if (!el) return;
        el.style.willChange = "opacity";
        el.style.transition = `opacity ${durationMs}ms ease`;
        el.style.opacity = "1";
        el.style.pointerEvents = "none"; // hindre nye klikk under fading
        requestAnimationFrame(() => {
            el.style.opacity = "0";
            setTimeout(() => {
                el.style.display = "none";
                el.style.transition = "";
                el.style.willChange = "";
            }, durationMs + 40);
        });
    }

    /* --------------------------- Map helpers ------------------------------ */
    const MapUtils = {
        setInteractive(map, enabled) {
            if (!map) return;
            const onoff = enabled ? "enable" : "disable";
            map.dragging[onoff]();
            map.scrollWheelZoom[onoff]();
            map.doubleClickZoom[onoff]();
            map.boxZoom[onoff]();
            map.keyboard[onoff]();
            if (map.tap) map.tap[onoff]();
        },

        ensureSize(map) {
            if (!map) return;
            setTimeout(() => map.invalidateSize(), 0);
        },

        addOsmLayer(map) {
            return L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
                maxZoom: 19,
                attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OSM</a>'
            }).addTo(map);
        },

        addRecenterControl(map, onRecenter) {
            const ctrl = L.control({ position: "topleft" });
            ctrl.onAdd = function () {
                const btn = L.DomUtil.create("button", "leaflet-bar");
                btn.type = "button";
                btn.title = "Sentrer kartet til min posisjon";
                btn.style.width = "40px";
                btn.style.height = "40px";
                btn.style.display = "grid";
                btn.style.placeItems = "center";
                btn.style.fontSize = "18px";
                btn.textContent = "⌖";
                L.DomEvent.on(btn, "click", (e) => {
                    L.DomEvent.stopPropagation(e);
                    onRecenter && onRecenter();
                });
                return btn;
            };
            ctrl.addTo(map);
            return ctrl;
        },

        geolocateOnce(onSuccess, onError) {
            if (!navigator.geolocation) {
                onError && onError(new Error("Geolocation not supported"));
                return;
            }
            navigator.geolocation.getCurrentPosition(
                (pos) => onSuccess && onSuccess(pos),
                (err) => onError && onError(err),
                { enableHighAccuracy: true, timeout: 7000, maximumAge: 0 }
            );
        }
    };
    ORS.MapUtils = MapUtils;

    /* ------------------------- DriftController ---------------------------- */
    function DriftController(map, options = {}) {
        this.map = map;
        this.rafId = null;
        this.startTs = 0;
        this.reduceMotion =
            window.matchMedia &&
            window.matchMedia("(prefers-reduced-motion: reduce)").matches;

        this.amplitudePx = options.amplitudePx ?? CONFIG.DRIFT_AMPLITUDE_PX;
        this.angularVelocity = options.angularVelocity ?? CONFIG.DRIFT_ANGULAR_VELOCITY;
        this.smoothing = options.smoothing ?? CONFIG.DRIFT_SMOOTHING;

        this._blendPoint = null;
    }

    DriftController.prototype._frame = function (ts) {
        if (!this.map) return;

        if (!this.startTs) this.startTs = ts;
        const t = (ts - this.startTs) / 1000.0;
        const angle = t * this.angularVelocity;

        const dx = Math.cos(angle) * this.amplitudePx;
        const dy = Math.sin(angle) * this.amplitudePx;

        const size = this.map.getSize();
        const centerPoint = L.point(size.x / 2, size.y / 2);
        const targetPoint = L.point(centerPoint.x + dx, centerPoint.y + dy);

        if (!this._blendPoint) this._blendPoint = centerPoint.clone();
        const a = this.smoothing;
        this._blendPoint = L.point(
            this._blendPoint.x + (targetPoint.x - this._blendPoint.x) * a,
            this._blendPoint.y + (targetPoint.y - this._blendPoint.y) * a
        );

        const targetLatLng = this.map.containerPointToLatLng(this._blendPoint);
        this.map.panTo(targetLatLng, { animate: false });

        this.rafId = window.requestAnimationFrame(this._frame.bind(this));
    };

    DriftController.prototype.start = function () {
        if (!this.map || this.reduceMotion || this.rafId) return;
        this.startTs = 0;
        this._blendPoint = null;
        this.rafId = window.requestAnimationFrame(this._frame.bind(this));
    };

    DriftController.prototype.stop = function () {
        if (this.rafId) {
            window.cancelAnimationFrame(this.rafId);
            this.rafId = null;
        }
        this._blendPoint = null;
    };

    /* ------------------------------ HomeGate ------------------------------ */
    ORS.HomeGate = (function () {
        function init() {
            if (!assertLeaflet()) return;

            const overlay = document.getElementById("gate-overlay");
            const btnFakeLogin = document.getElementById("btnFakeLogin");
            const btnFakeRegister = document.getElementById("btnFakeRegister");
            const mapEl = document.getElementById("map-home");
            if (!overlay || !mapEl) {
                console.warn("[ORS] Mangler #gate-overlay eller #map-home i DOM.");
                return;
            }

            // Opprett Leaflet-kart
            const map = L.map("map-home", {
                zoomControl: true,
                attributionControl: true
            });

            MapUtils.addOsmLayer(map);
            map.setView(CONFIG.FALLBACK_CENTER, CONFIG.FALLBACK_ZOOM);

            // geoHome: første vellykkede geolokasjon – brukes ved login!
            let geoHome = null;      // { lat, lng } settes én gang
            let hasCentered = false; // kun UI-zoom logikk

            // Første geo-forsøk – sett geoHome KUN første gang
            MapUtils.geolocateOnce(
                (pos) => {
                    const { latitude, longitude } = pos.coords;
                    if (!geoHome) {
                        geoHome = { lat: latitude, lng: longitude }; // LAGRER HOME ÉN GANG
                    }
                    map.setView([latitude, longitude], CONFIG.GEO_ZOOM);
                    hasCentered = true;
                    L.circleMarker([latitude, longitude], { radius: 6 }).addTo(map);
                },
                () => { /* behold fallback */ }
            );

            // "Sentrer meg" – påvirker ikke geoHome (den skal være konstant)
            MapUtils.addRecenterControl(map, () => {
                MapUtils.geolocateOnce(
                    (pos) => {
                        const { latitude, longitude } = pos.coords;
                        map.setView([latitude, longitude], hasCentered ? map.getZoom() : CONFIG.GEO_ZOOM);
                        hasCentered = true;
                    },
                    () => { }
                );
            });

            // Lås kartet før login + start drift
            overlay.style.display = "grid";
            overlay.style.opacity = "1";
            MapUtils.setInteractive(map, false);

            const drift = new DriftController(map);
            drift.start();

            // Robust re-senter + fade + unlock, uansett geo-tilgang
            function recenterToTargetThenUnlock(targetLat, targetLng, targetZoom) {
                // 1) Stopp drift og ev. pågående Leaflet-anim/pan
                drift.stop();
                map.stop();

                // 2) midlertidig disable interaksjon mens vi flytter
                MapUtils.setInteractive(map, false);

                const finish = () => {
                    MapUtils.setInteractive(map, true);
                    MapUtils.ensureSize(map);
                    fadeOut(overlay, CONFIG.OVERLAY_FADE_MS);
                };

                // 3) Hvis vi allerede står "på" målet (innen epsilon) ⇒ finish direkte
                const cur = map.getCenter();
                const dz = map.getZoom();
                const latDiff = Math.abs(cur.lat - targetLat);
                const lngDiff = Math.abs(cur.lng - targetLng);
                const samePlace = latDiff < CONFIG.CENTER_EPSILON_DEG && lngDiff < CONFIG.CENTER_EPSILON_DEG;
                const sameZoom = dz === targetZoom;

                if (samePlace && sameZoom) {
                    finish();
                    return;
                }

                // 4) Fly dit; sikkerhetsnett i tilfelle moveend ikke trigges
                let safety = setTimeout(() => {
                    map.off("moveend", onMoveEnd);
                    finish();
                }, CONFIG.MOVEEND_FAILSAFE_MS);

                function onMoveEnd() {
                    clearTimeout(safety);
                    finish();
                }

                map.once("moveend", onMoveEnd);
                map.flyTo([targetLat, targetLng], targetZoom, {
                    animate: true,
                    duration: CONFIG.FLY_DURATION,
                    easeLinearity: 0.25
                });
            }

            // Ved "Login": velg mål = geoHome hvis finnes, ellers fallback (ikke prøv geo igjen)
            function onLogin() {
                const target = geoHome
                    ? { lat: geoHome.lat, lng: geoHome.lng, zoom: CONFIG.GEO_ZOOM }
                    : { lat: CONFIG.FALLBACK_CENTER[0], lng: CONFIG.FALLBACK_CENTER[1], zoom: CONFIG.GEO_ZOOM };

                recenterToTargetThenUnlock(target.lat, target.lng, target.zoom);
            }

            if (btnFakeLogin) btnFakeLogin.addEventListener("click", onLogin);
            if (btnFakeRegister) btnFakeRegister.addEventListener("click", onLogin);

            // Size-fix
            window.addEventListener("resize", debounce(() => MapUtils.ensureSize(map), 150));
            setTimeout(() => MapUtils.ensureSize(map), 50);
        }

        return { init };
    })();

    /* ----------------------------- Global hook ---------------------------- */
    window.initHomeGate = function () {
        try {
            ORS.HomeGate.init();
        } catch (err) {
            console.error("[ORS] initHomeGate feilet:", err);
        }
    };

})(window, document);
