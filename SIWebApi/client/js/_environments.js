(function (window) {
    var development = ['localhost'];
    var stage = ['siwebapi.azurewebsites.net'];
    var produzione = ['siwebapi.azurewebsites.net'];

    if (development.indexOf(window.location.hostname) >= 0) {
        window._endpoint = {
            service: 'http://localhost:3566'
        };
        return;
    }

    if (stage.indexOf(window.location.hostname) >= 0) {
        window._endpoint = {
            service: ''
        };
        return;
    }

    if (produzione.indexOf(window.location.hostname) >= 0) {
        window._endpoint = {
            service: ''
        };
        return;
    }
}(this));