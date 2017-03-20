var states = [
        { name: 'public', state: { templateUrl: '/client/app/templates/main.html', data: { bodyLayout: '' } } },
        { name: 'public.home', state: { url: '/home', templateUrl: '/client/html/home.html' } },
        { name: 'public.privacy', state: { url: '/privacy', templateUrl: '/client/html/privacy.html' } },
        { name: 'public.terms', state: { url: '/terms', templateUrl: '/client/html/terms.html' } },
        { name: 'public.faq', state: { url: '/faq', templateUrl: '/client/html/faq.html' } },
        { name: 'private.user', state: { url: '/user', templateUrl: '/client/html/user.html' } },
        { name: 'public.login', state: { url: '/login', templateUrl: '/client/html/login.html', data: { bodyLayout: 'hold-transition empty-layout' } } },
        { name: 'public.forgot-password', state: { url: '/forgot-password', templateUrl: '/client/html/forgotPassword.html' } },
        { name: 'public.register', state: { url: '/register', templateUrl: '/client/html/register.html' } },
        { name: 'public.confirmEmail', state: { url: '/confirmEmail?idUser&code', templateUrl: '/client/html/confirmEmail.html' } },
        { name: 'public.resetPassword', state: { url: '/resetPassword?idUser&code', templateUrl: '/client/html/resetPassword.html' } }

];
var siwebapi = angular.module('siwebapi', [
    "ui.router",
    "ngRoute",
    "ngResource",
    "ngSanitize",
    'ngAnimate',
    "ui.bootstrap",
    "ui.bootstrap.datetimepicker",
    "ui.router",
    "ui.grid",
    "ui.grid.selection",
    "ui.grid.pagination",
    "ui.grid.grouping",
    "ui.grid.edit",
    "ui.sortable",
    "smart-table"
])
.config(['$stateProvider'
       , '$urlRouterProvider'
       , '$locationProvider'
       , function ($stateProvider
                 , $urlRouterProvider
                 , $locationProvider) {
           $locationProvider.html5Mode(false);
           angular.forEach(states, function (state) {
               $stateProvider.state(state.name, state.state);
           });
       }])
    //.run(function ($state, LoginService) {
    //    if (LoginService.userInfo()) {
    //        $state.go("private.user");
    //    }
    //    else {
    //        $state.go("public.home");
    //    }
    //})

.config(["$stateProvider", "$urlRouterProvider", "$httpProvider", "$qProvider", function ($stateProvider, $urlRouterProvider, $httpProvider, $qProvider) {
    $httpProvider.interceptors.push("AuthHttpResponseInterceptor");
   
    //$stateProvider.state('public', {
    //    name: 'public',
    //    url: '/public',
    //        views: {
    //      "main": { templateUrl: '/client/app/templates/main.html' }
    //      }
    //   })
    //.state('public.home', {
    //    name: 'public.home',
    //    url: '/home',
    //    views: { "top-menu": { templateUrl: '/client/html/home.html' } }
    //})
    //.state('public.login', {
    //    name: 'public.login',
    //    url: '/user',
    //    views: {
    //        "main": { templateUrl: '/client/app/templates/user.html' }
    //    }
    //})
    //$urlRouterProvider.otherwise('/client/public/home');

}])
 .factory('AuthHttpResponseInterceptor', ['$q', '$location', '$rootScope', function ($q, $location, $rootScope) {
     var waiters = 0;
     return {
         request: function (config) {
         if ($('body').hasClass("wait") == false){
            $('body').addClass("wait");
         }
         waiters++;
         if (config.url.indexOf('/api') >= 0){
            config.url = window._endpoint.service + config.url;
         }
             
        config.headers = config.headers || {};
        var authData = JSON.parse(window.localStorage.getItem("authorizationData") || null);
        if (authData) {
            config.headers.Authorization = 'Bearer ' + authData.token;
        }
        return config;
      },
       response: function (response) {
        waiters--;
        if (waiters == 0 && $('body').hasClass("wait"))
            $('body').removeClass("wait")
        return response || $q.when(response);
      },
      responseError: function (rejection) {
        waiters--;
        if (waiters == 0 && $('body').hasClass("wait"))
            $('body').removeClass("wait");

       if (rejection.status === 401)
            $location.path("/");
        return $q.reject(rejection);
      }
     
    }
  }])