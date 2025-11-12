document.addEventListener("DOMContentLoaded", function () {

    var tokenElement = document.getElementsByName('__RequestVerificationToken')[0];
    var token = null;

    if (tokenElement) {
        token = tokenElement.value;
        console.log("Anti-forgery token funnet!"); 
    } else {
        console.error("KRITISK FEIL: Fant ikke __RequestVerificationToken. Sjekk at @Html.AntiForgeryToken() er i _Layout.cshtml.");
    }

    var knapp = document.getElementById("hurtigRegistrerKnapp");

    if (knapp) {
        knapp.addEventListener("click", function (e) {
            e.preventDefault(); 

            if (!token) {
                alert("En feil oppstod: Sikkerhets-token mangler. Kan ikke registrere. (Se konsoll for detaljer)");
                return;
            }

            if (navigator.geolocation) {
                navigator.geolocation.getCurrentPosition(
                    posisjon => sendPosisjonTilServer(posisjon, token),
                    visGeolokasjonsFeil
                );
            } else {
                alert("Geolokasjon støttes ikke av denne nettleseren.");
            }
        });
    }
});

function sendPosisjonTilServer(posisjon, token) { 
    var lat = posisjon.coords.latitude;
    var lon = posisjon.coords.longitude;
    var lokasjonStreng = `Lat: ${lat.toFixed(6)}, Lon: ${lon.toFixed(6)}`;

    console.log("Sender denne posisjonen:", lokasjonStreng);

    fetch('/Hinder/HurtigRegistrer', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': token 
        },
        body: 'lokasjon=' + encodeURIComponent(lokasjonStreng)
    })
        .then(response => {
            if (response.ok) {
                alert("Hinder hurtigregistrert!\nDu kan fullføre registreringen under 'Uferdige hinder'.");
                window.location.reload();
            } else {
                alert("En feil oppstod på serveren.");
            }
        })
        .catch(error => {
            console.error('Feil:', error);
            alert("En feil oppstod under sending.");
        });
}

function visGeolokasjonsFeil(error) {
    switch (error.code) {
        case error.PERMISSION_DENIED:
            alert("Du må tillate tilgang til posisjonen din for å bruke hurtigregistrering.");
            break;
        case error.POSITION_UNAVAILABLE:
            alert("Posisjonsinformasjon er ikke tilgjengelig.");
            break;
        case error.TIMEOUT:
            alert("Forespørselen om posisjon utløp på tid.");
            break;
        default:
            alert("En ukjent feil oppstod ved henting av posisjon.");
            break;
    }
}

function parseLokasjon(lokasjonStreng) {
    if (!lokasjonStreng || !lokasjonStreng.startsWith("Lat:")) {
        console.warn("Ugyldig lokasjonsstreng, bruker standardposisjon.");
        return [58.1463, 7.9952]; 
    }

    try {
        var parts = lokasjonStreng.split(',');
        var lat = parseFloat(parts[0].split(':')[1].trim());
        var lon = parseFloat(parts[1].split(':')[1].trim());
        return [lat, lon];
    } catch (e) {
        console.error("Klarte ikke parse lokasjon:", lokasjonStreng);
        return [58.1463, 7.9952]; 
    }
}