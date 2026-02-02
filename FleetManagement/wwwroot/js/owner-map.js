const map = L.map('map').setView([50.3, 19.0], 8);

L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    attribution: '&copy; OpenStreetMap contributors'
}).addTo(map);

// wymuszenie przeliczenia rozmiaru mapy po załadowaniu
setTimeout(() => map.invalidateSize(), 100);

let marker = null;

function requestLocation(vehicleId, minutes) {
    fetch('/Location/Request', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ vehicleId: 12, minutes:30 })
    });
}

function refreshLocation(vehicleId) {
    fetch(`/Location/Latest?vehicleId=${vehicleId}`)
        .then(r => r.json())
        .then(loc => {
            if (!loc) return;

            if (marker) map.removeLayer(marker);
            marker = L.marker([loc.latitude, loc.longitude]).addTo(map);
            map.setView([loc.latitude, loc.longitude], 13);
        });
}

// odświeżanie co 10s
setInterval(() => refreshLocation(12), 10000);
