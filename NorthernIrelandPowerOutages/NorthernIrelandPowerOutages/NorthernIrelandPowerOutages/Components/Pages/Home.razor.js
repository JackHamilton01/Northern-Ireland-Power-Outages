let markersOnMap = [];
let googleMap;
let isGeoJsonApplied = false;

export function initMap(markers, dotNetHelper) {
    const bounds = {
        north: 59.0,   // Northern Scotland
        south: 49.8,   // Southern England
        west: -11.0,   // West of Ireland
        east: 2.0      // East of UK
    };

    googleMap = new google.maps.Map(document.getElementById("map"), {
        restriction: {
            latLngBounds: bounds,
            strictBounds: true
        },
        gestureHandling: "greedy",
        zoom: 8,
        center: { lat: 54.6079, lng: -5.9264 }
    });

    googleMap.fitBounds(bounds);

    //const infoWindow = new google.maps.InfoWindow();

    markers.forEach(marker => {
        const markerOptions = {
            position: { lat: marker.latitude, lng: marker.longitude },
            map: googleMap,
            title: marker.Name,
            icon: {
                url: "/Images/Map/Default.png",
                size: new google.maps.Size(30, 30),
                scaledSize: new google.maps.Size(30, 30)
            }
        };

        const mapMarker = new google.maps.Marker(markerOptions);
        markersOnMap.push(mapMarker);

        mapMarker.addListener("click", function () {
            //        const dynamicContent = `
            //<h3>${marker.name}</h3>
            //<p>Details: ${marker.details}</p>
            //`;
            //        infoWindow.setContent(dynamicContent);
            //        infoWindow.open(googleMap, mapMarker);

            googleMap.panTo(mapMarker.getPosition());

            dotNetHelper.invokeMethodAsync('OnMarkerClicked', marker);
        });
    });

    window.zoomMap = function (zoomChange) {
        let currentZoom = googleMap.getZoom();
        googleMap.setZoom(currentZoom + zoomChange);
    };
};

export function toggleGeoJson(countyOutageInformation) {
    console.log(countyOutageInformation);
    try {
        if (!isGeoJsonApplied) {

            toggleMapMarkersVisibility(null);

            googleMap.data.loadGeoJson("Map/NorthernIrelandCounties.geojson");

            updateCountyOutages(countyOutageInformation);
            isGeoJsonApplied = true;
        }
        else {
            toggleMapMarkersVisibility(googleMap);

            googleMap.data.forEach(function (feature) {
                googleMap.data.remove(feature);
            });

            isGeoJsonApplied = false;
        }
    } catch (error) {
        console.log(error);
    }
}

function updateCountyOutages(outageData) {
    const normalizedData = normalizeOutageData(outageData);

    const values = Object.values(normalizedData);
    const min = Math.min(...values);
    const max = Math.max(...values);

    googleMap.data.setStyle(function (feature) {
        const countyName = feature.getProperty("CountyName")?.toLowerCase();

        console.log(countyName);
        let count;
        if (countyName === "londonderry") {
            count = normalizedData["derry/londonderry"];
        } else {
            count = normalizedData[countyName];
        }

        const color = count !== undefined
            ? getColorForOutageCount(count, min, max)
            : "#dddddd"; // fallback color

        return {
            fillColor: color,
            fillOpacity: 0.6,
            strokeColor: "black",
            strokeWeight: 2,
        };
    });
}

function normalizeOutageData(outageData) {
    const normalized = {};
    for (const key in outageData) {
        normalized[key.toLowerCase()] = outageData[key];
    }
    return normalized;
}

function getColorForOutageCount(count, min, max) {
    if (max === min) return "#cccccc"; // fallback if all values are the same

    const t = (count - min) / (max - min); // normalize 0–1

    const r = Math.round(255 * t);         // red increases with count
    const g = Math.round(255 * (1 - t));                           // keep green at 0
    const b = 0;   // blue decreases with count

    return `rgb(${r}, ${g}, ${b})`;
}

function toggleMapMarkersVisibility(map) {
    for (let i = 0; i < markersOnMap.length; i++) {
        markersOnMap[i].setMap(map);
    }
}

export function logToConsole(message) {
    console.log(message);
}
