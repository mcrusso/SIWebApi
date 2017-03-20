siwebapi.factory('LoginService', function ($q, $http, ngAuthSettings, serveruri) {
  var authorize = false;
  var loginServices = {};

  loginServices.getToken = function (credentials) {
    var d = $q.defer();

    if (this.isAuthenticated()) {
        d.resolve(window.sessionStorage.getItem("token"));
      return d.promise;
    }

    var query = {
      method: "POST",
      url: "/api/auth",
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
      data: $.param({ Email: credentials.username, Password: credentials.password, RememberMe: credentials.rememberMe }),
    };

    $http(query).then(function (result) {
      token = result.data;
      window.sessionStorage.setItem("token", JSON.stringify(token));
        //SessionStorage.set("token", token);
      d.resolve(token);
    }, function (error) {
      console.log(error);
      d.reject(error);
    });

    return d.promise;
  };

  loginServices.getUserInfo = function () {
    var d = $q.defer();

    if (!this.isAuthenticated()) {
      d.reject("Token is empty");
      return d.promise;
    }

    if (window.sessionStorage.getItem("userInfo")) {
      d.resolve(window.sessionStorage.getItem("userInfo"));
      return d.promise;
    }

    var query = apiCall("/api/account/users/info", "GET")
    $http(query).then(function (res) {
        window.sessionStorage.setItem("userInfo", window.localStorage.setItem(JSON.stringify(res.data)));
      //SessionStorage.set("userInfo", res.data);
      d.resolve(res.data);
    }, function (error) {
      d.reject(error);
    });
    return d.promise;
  };

  loginServices.login = function (credentials) {
    var d = $q.defer();
    var promise = [];
    this.getToken(credentials).then(function (token) {
      $http(apiCall("/api/account/users/info", "GET")).then(function (res) {
        window.sessionStorage.setItem("userInfo", (JSON.stringify(res.data)));
        //SessionStorage.set("userInfo", res.data);
        d.resolve(res);
      },
      function (error) {
        console.log(error);
        d.reject(error);
      });
    },
    function (error) {
      console.log(error);
      d.reject(error);
    });
    return d.promise;
  };

  loginServices.logout = function () {
    var query = apiCall("/api/account/logout", "GET")
    return $http(query).then(function (res) {
      token = null;
      var user = loginServices.userInfo();
      window.sessionStorage.removeItem("token");
      window.sessionStorage.removeItem("userInfo");
      window.sessionStorage.removeItem("authorized_" + user.id);
     
      return res;
    }, function (error) {
      token = null;
      var user = loginServices.userInfo();
      window.sessionStorage.removeItem("token");
      window.sessionStorage.removeItem("userInfo");
      window.sessionStorage.removeItem("authorized_" + user.id);
      console.log(error);
      return error;
    });
  };

  loginServices.isAuthenticated = function () {
    var token = window.sessionStorage.getItem("token");
    if (!token)
      return false;
    return !tokenIsExpired(token);
  };

  loginServices.authorized = function (idUser, appKey) {
      var d = $q.defer();

      if (!this.isAuthenticated()) {
          d.reject("Token is empty");
          return d.promise;
      }

      if (!idUser) {
          var user = loginServices.userInfo();
          if (!user) {
              d.reject("Not valid user authenticated");
              return d.promise;
          }
          idUser = user.Id;
      }

      if (!appKey) {
          d.reject("Application Key unvalid");
          return d.promise;
      }

      $http(apiCall("/api/auth/app/" + appKey, "GET")).then(
      function (res) {
          window.sessionStorage.setItem("authorized_" + idUser, (JSON.stringify(res.data)));
          //SessionStorage.set("authorized_" + idUser, res.data);
          d.resolve(res.data);
      },
      function (error) {
          d.reject(error);
      });
      return d.promise;
  };

  loginServices.isAuthorized = function (idUser, appKey) {
      if (!this.isAuthenticated()) {
          return "Token is empty";
      }

      if (!idUser) {
          var user = loginServices.userInfo();
          idUser = user.Id;
      }

      var authorize = false;
      var store = window.sessionStorage.getItem("authorized_"+idUser);
      //var store = SessionStorage.get("authorized_" + idUser);
      if (store) authorize = store;
      return authorize;
  };

  loginServices.isInRole = function (role) {
      if (!role)
          return false;

      var user = loginServices.userInfo();
      if (!user || !role)
          return false;
      return Enumerable.From(user.Roles).Where("$.Name == '" + role + "'").ToArray().length > 0;
  };

  loginServices.token = function () {
      return window.sessionStorage.getItem("token");
      //return SessionStorage.get("token")
  };

  loginServices.userInfo = function () {
      return window.sessionStorage.getItem("userInfo");
      //return SessionStorage.get("userInfo");
  };

  function apiCall(serviceUrl, verb, parameters, body) {
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

  function tokenIsExpired(token) {
      var expire = new Date(token.expires);
      return expire <= new Date();
  }

 
   

         

  return loginServices;
});