let watchId = null;

function startSharing() {
    watchId = navigator.geolocation.watchPosition(
        pos => {
            fetch('/Location/Report', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    latitude: pos.coords.latitude,
                    longitude: pos.coords.longitude
                })
            });
        },
        err => alert("Location permission denied"),
        {
            enableHighAccuracy: true,
            maximumAge: 5000,
            timeout: 10000
        }
    );
}
