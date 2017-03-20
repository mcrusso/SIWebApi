siwebapi.controller('AppController', function ($scope, $state, $stateParams, $rootScope) {
    $scope.credentials = {
        username: '',
        password: '',
        rememberMe: false
    };
})