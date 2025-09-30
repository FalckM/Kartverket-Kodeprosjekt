document.addEventListener("DOMContentLoaded", function () { //Ben
    var map = L.map('map').setView([59.91, 10.75], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors'
    }).addTo(map);

    var objects = [];
    var currentMode = 'punkt';
    var tempLine = null;
    var tempLinePoints = [];

    window.setMode = function (mode) {
        currentMode = mode;
        tempLine = null;
        tempLinePoints = [];
        alert("Modus satt til: " + mode);
    };

    function promptForInfo() {
        let navn = prompt("Navn:");
        if (!navn) return null;
        let hoyde = prompt("Høyde:");
        let bredde = prompt("Bredde:");
        let beskrivelse = prompt("Beskrivelse:");
        return { Navn: navn, Hoyde: hoyde, Bredde: bredde, Beskrivelse: beskrivelse };
    }

    function createPopupContent(obj) {
        return `
            <b>${obj.details.Navn}</b><br>
            Høyde: ${obj.details.Hoyde}<br>
            Bredde: ${obj.details.Bredde}<br>
            Beskrivelse: ${obj.details.Beskrivelse}<br>
            <button id="delete-${obj._leaflet_id}">Slett</button>
        `;
    }

    function addMarker(latlng, details) {
        var marker = L.marker(L.latLng(latlng.lat, latlng.lng), { draggable: true }).addTo(map);
        marker.details = details;
        marker.bindPopup(createPopupContent(marker));
        marker.on('popupopen', function () {
            document.getElementById(`delete-${marker._leaflet_id}`).onclick = function () {
                map.removeLayer(marker);
                objects = objects.filter(o => o !== marker);
            };
        });
        objects.push(marker);
    }

    function addPolyline(latlngs, details) {
        var lineLatLngs = latlngs.map(p => L.latLng(p.lat, p.lng));
        var poly = L.polyline(lineLatLngs, { color: 'red' }).addTo(map);
        poly.details = details;
        poly.bindPopup(createPopupContent(poly));
        poly.on('popupopen', function () {
            document.getElementById(`delete-${poly._leaflet_id}`).onclick = function () {
                map.removeLayer(poly);
                objects = objects.filter(o => o !== poly);
            };
        });
        objects.push(poly);
    }

    map.on('click', function (e) {
        if (currentMode === 'punkt') {
            let info = promptForInfo();
            if (!info) return;
            addMarker(e.latlng, info);
        } else if (currentMode === 'linje') {
            tempLinePoints.push(e.latlng);
            if (!tempLine) {
                tempLine = L.polyline(tempLinePoints, { color: 'red' }).addTo(map);
            } else {
                tempLine.setLatLngs(tempLinePoints);
            }

            if (tempLinePoints.length >= 2) {
                let info = promptForInfo();
                if (!info) return;
                addPolyline(tempLinePoints, info);
                tempLine = null;
                tempLinePoints = [];
            }
        }
    });

    window.saveMap = function () {
        // Lag en korrekt strukturert liste for backend
        var data = objects.map(o => {
            if (o instanceof L.Marker) {
                return {
                    Type: 'punkt',
                    LatLng: { lat: o.getLatLng().lat, lng: o.getLatLng().lng },
                    LatLngs: null,
                    Details: o.details
                };
            } else if (o instanceof L.Polyline) {
                return {
                    Type: 'linje',
                    LatLng: null,
                    LatLngs: o.getLatLngs().map(p => ({ lat: p.lat, lng: p.lng })),
                    Details: o.details
                };
            }
        });

        fetch('/Map/Save', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        }).then(res => {
            if (res.ok) alert("Kartdata lagret!");
            else alert("Noe gikk galt ved lagring.");
        });
    };

    fetch('/Map/Load')
        .then(res => res.json())
        .then(data => {
            data.forEach(o => {
                if (o.Type === 'punkt') addMarker(o.LatLng, o.Details);
                else if (o.Type === 'linje') addPolyline(o.LatLngs, o.Details);
            });
        });
});
