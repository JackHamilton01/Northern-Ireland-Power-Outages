let markersOnMap = [];
let googleMap;
let isGeoJsonApplied = false;
let markerCluster = null;
const countyLabels = [];
let customOverlays = [];

export function initMap(markers, dotNetHelper) {
    try {
        const bounds = {
            north: 59.0,
            south: 49.8,
            west: -11.0,
            east: 2.0
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

        // Clear previous markers
        markersOnMap.length = 0;

        // Create markers WITHOUT attaching to map
        markers.forEach(marker => {
            var iconUrl = getMarkerIconUrl(marker.icon);

            const markerOptions = {
                position: { lat: marker.latitude, lng: marker.longitude },
                title: marker.Name,
                icon: {
                    url: iconUrl,
                    size: new google.maps.Size(30, 30),
                    scaledSize: new google.maps.Size(30, 30)
                }
                // <-- no map property here
            };

            const mapMarker = new google.maps.Marker(markerOptions);
            markersOnMap.push(mapMarker);

            mapMarker.addListener("click", function () {
                googleMap.panTo(mapMarker.getPosition());
                dotNetHelper.invokeMethodAsync('OnMarkerClicked', marker);
            });
        });

        console.log('markerClusterer:', MarkerClusterer);

        // Use the correct constructor based on your MarkerClusterer version

        // If classic MarkerClusterer (loaded via script):
        markerCluster = new MarkerClusterer(googleMap, markersOnMap, {
            styles: configureClusterStyle()
        });

        // If new MarkerClusterer (ES module), use this instead:
        // const markerCluster = new MarkerClusterer({ map: googleMap, markers: markersOnMap });

        window.zoomMap = function (zoomChange) {
            let currentZoom = googleMap.getZoom();
            googleMap.setZoom(currentZoom + zoomChange);
        };
    } catch (error) {
        console.error("Failed to initialize marker clustering:", error);
    }
}

function clearAllCustomOverlays() {
    for (const overlay of customOverlays) {
        overlay.setMap(null); // triggers onRemove
    }
    customOverlays = []; // clear array
}


function getMarkerIconUrl(iconName) {
    let iconUrl;

    if (iconName === "Default") {
        iconUrl = "/Images/Map/Default.png";
    }
    else if (iconName === "Home") {
        iconUrl = "/Images/Map/Home.png";
    }
    else if (iconName === "Favourite") {
        iconUrl = "/Images/Map/Favourite.png";
    }
    else if (iconName === "Planned") {
        iconUrl = "/Images/Map/Planned.png";
    }
    else {
        iconUrl = "/Images/Map/Default.png";
    }

    return iconUrl;
}


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

            clearAllCustomOverlays();

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

    googleMap.data.setStyle(feature => {
        const countyName = feature.getProperty("CountyName")?.toLowerCase();
        let count;
        if (countyName === "londonderry") {
            count = normalizedData["derry/londonderry"];
        } else {
            count = normalizedData[countyName];
        }

        let customLatLng;
        console.log(countyName);
        if (countyName === "antrim") {
            customLatLng = { lat: 54.900636, lng: -6.193263 };
        }
        else if (countyName === "armagh") {
            customLatLng = { lat: 54.352892, lng: -6.583227 };
        }
        if (countyName === "down") {
            customLatLng = { lat: 54.370496, lng: -5.985898 };
        }
        if (countyName === "fermanagh") {
            customLatLng = { lat: 54.371296, lng: -7.655607 };
        }
        if (countyName === "tyrone") {
            customLatLng = { lat: 54.615698, lng: -7.118424 };
        }
        if (countyName === "derry/londonderry") {
            customLatLng = { lat: 54.915075, lng: -6.786182 };
        }
        addCustomOverlay(customLatLng.lat, customLatLng.lng, `
          <div style="
            width: 20px;
            height: 20px;
            border-radius: 50%;
            background-color: #1E90FF;
            color: white;
            font-weight: bold;
            display: flex;
            align-items: center;
            justify-content: center;
            box-shadow: 0 0 6px rgba(0,0,0,0.3);
            font-size: 12px;
            ">
            ${count}
          </div>
        `);

        const color = count !== undefined
            ? getColorForOutageCount(count, min, max)
            : "#dddddd";

        return {
            fillColor: color,
            fillOpacity: 0.6,
            strokeColor: "white",
            strokeWeight: 2,
        };
    });
}

function addCustomOverlay(lat, lng, html) {
    const CustomOverlay = function (position, htmlContent, map) {
        this.position = position;
        this.content = htmlContent;
        this.map = map;
        this.div = null;
        this.setMap(map);
    };

    CustomOverlay.prototype = new google.maps.OverlayView();

    CustomOverlay.prototype.onAdd = function () {
        const div = document.createElement("div");
        div.style.position = "absolute";
        div.innerHTML = this.content;
        this.div = div;

        const panes = this.getPanes();
        panes.overlayMouseTarget.appendChild(div);
    };

    CustomOverlay.prototype.draw = function () {
        const projection = this.getProjection();
        const pos = projection.fromLatLngToDivPixel(this.position);

        if (this.div) {
            this.div.style.left = pos.x + "px";
            this.div.style.top = pos.y + "px";
        }
    };

    CustomOverlay.prototype.onRemove = function () {
        if (this.div) {
            this.div.remove();
            this.div = null;
        }
    };

    const pos = new google.maps.LatLng(lat, lng);
    const overlay = new CustomOverlay(pos, html, googleMap);

    customOverlays.push(overlay); // store reference
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

function toggleMapMarkersVisibility(visible) {
    if (visible) {
        markerCluster.addMarkers(markersOnMap);
    } else {
        markerCluster.clearMarkers();
    }
}


export function updateMarkers(markers, dotNetHelper) {
    if (!googleMap) {
        return;
    }

    // Remove existing markers
    markersOnMap.forEach(marker => marker.setMap(null));
    markersOnMap = [];

    // Add new markers
    markers.forEach(marker => {
        var iconUrl = getMarkerIconUrl(marker.icon);

        const markerOptions = {
            position: { lat: marker.latitude, lng: marker.longitude },
            map: googleMap,
            title: marker.Name,
            icon: {
                url: iconUrl,
                size: new google.maps.Size(30, 30),
                scaledSize: new google.maps.Size(30, 30)
            }
        };

        const mapMarker = new google.maps.Marker(markerOptions);
        markersOnMap.push(mapMarker);

        mapMarker.addListener("click", function () {
            googleMap.panTo(mapMarker.getPosition());
            dotNetHelper.invokeMethodAsync('OnMarkerClicked', marker);
        });
    });

    markerCluster = new MarkerClusterer(googleMap, markersOnMap, {
        styles: configureClusterStyle()
    });
}

export function MoveToLocation(latitude, longitude) {
    const location = new google.maps.LatLng(latitude, longitude);
    googleMap.panTo(location);
    googleMap.setZoom(17);
}

export function logToConsole(message) {
    console.log(message);
}

function configureClusterStyle() {
    return [
        {
            textColor: 'white',
            textSize: 14,
            url: 'https://developers.google.com/maps/documentation/javascript/examples/markerclusterer/m1.png',
            height: 53,
            width: 53
        },
        {
            textColor: 'white',
            textSize: 16,
            url: 'https://developers.google.com/maps/documentation/javascript/examples/markerclusterer/m2.png',
            height: 56,
            width: 56
        },
        {
            textColor: 'white',
            textSize: 18,
            url: 'https://developers.google.com/maps/documentation/javascript/examples/markerclusterer/m3.png',
            height: 66,
            width: 66
        }
    ];
}