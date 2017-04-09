siwebapi.factory('CallerService', function ($q, $http, ngAuthSettings, serveruri) {
  
  var callerServices = {};
  callerServices.apiCall = function (serviceUrl, verb, parameters, body) {
    var query = {
        method: verb,
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        url: serviceUrl,
        params: parameters
    };

    if (body)
        query.data = $.param(body);

    if (loginServices.isAuthenticated() && loginServices.token()) {
        query.headers.Authorization = "bearer " + loginServices.token().access_token;
    }
    return query;
 };
  return callerServices;
});