export function initMap(markers, dotNetHelper) {
    const bounds = {
        north: 59.0,   // Northern Scotland
        south: 49.8,   // Southern England
        west: -11.0,   // West of Ireland
        east: 2.0      // East of UK
    };

    const googleMap = new google.maps.Map(document.getElementById("map"), {
        restriction: {
            latLngBounds: bounds,
            strictBounds: true
        },
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

export function logToConsole(message) {
    console.log(message);
}
