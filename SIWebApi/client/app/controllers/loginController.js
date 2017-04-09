siwebapi.controller('LoginController', function ($scope, $state, $stateParams, $rootScope, LoginService, $location, AUTH_EVENTS, LoginService) {
    $scope.credentials = {
        username: '',
        password: '',
        rememberMe: false
    };

    $scope.registration = {
        FirstName: null,
        LastName: null,
        Email: null,
        Password: null,
        ConfirmPassword: null,
        TermOfService: false,
        Privacy: false
    };

    $scope.login = function (credentials) {
        LoginService.login(credentials).then(function (user) {
            $rootScope.$broadcast(AUTH_EVENTS.loginSuccess, user.data);
        }, function (error) {
            $rootScope.$broadcast(AUTH_EVENTS.loginFailed);
            console.log(error);
            if (error && error.status == 403) {
                swal({
                    title: error.statusText,
                    text: error.data,
                    type: "info",
                    showCancelButton: true,
                    closeOnConfirm: false,
                    showLoaderOnConfirm: true,
                    cancelButtonText: "Ok",
                    confirmButtonText: "Invia nuovamente!",
                    html: true
                }, function () {
                    LoginService.sendConfirmEmail(credentials.username).then(function (result) {
                        swal({
                            title: "E-Mail di conferma inviata!",
                            text: "<strong>Una E-Mail per confermare il tuo account è stata inviata!</strong><br />Controlla la tua E-Mail per confermare il tuo accounte e completare la registrazione.",
                            type: "success",
                            html: true,
                        }, function () {
                            $state.go("public.login");
                        });
                    },
                    function (error) {
                        console.log(error);
                        LoginService.showMessage("Invio E-Mail di conferma fallito!", error);
                    })
                });
            }
            else {
                swal(error.statusText, error.data, "info");
            }
        });
    };

    $scope.logout = function () {
        LoginService.logout().then(function () {
            $rootScope.$broadcast(AUTH_EVENTS.logoutSuccess);
            $rootScope.$broadcast(AUTH_EVENTS.notAuthenticated);
        });
    };

    $scope.register = function (registered) {
        LoginService.register(registered).then(function (r) {
            swal({
                title: "Registrazione effettuata!",
                text: "<strong>Il tuo account è stato creato con successo!</strong><br />Controlla la tua E-Mail per confermare il tuo accounte e completare la registrazione.",
                type: "success",
                html: true
            }, function () {
                $state.go("public.login");
            });
        }, function (error) {
            console.log(error);
            LoginService.showMessage("Registrazione fallita!", error);
            
        });
    };

   
    $scope.forgotPassword = function (username) {
        LoginService.forgotPassword(username).then(function (result) {
            swal({
                title: "Richiesta di reset password inviata!",
                text: "<strong>Una E-Mail di reset password del tuo account è stata inviata!</strong><br />Controlla la tua E-Mail per completare il reset password.",
                type: "success",
                html: true,
            }, function () {
                $state.go("public.login");
            });
        },
        function (error) {  
            console.log(error);
            LoginService.showMessage("Richiesta di reset password fallita!", error);
                    });
    }

      

    $scope.resetPassword = function (password) {
        LoginService.resetPassword($stateParams.idUser, $stateParams.code, password).then(function (result) {
            swal({
                title: "Password impostata!",
                text: "<strong>Password resettata con successo!</strong><br />Accedi direttamente dall'applicazione!",
                type: "success",
                html: true
            }, function () {
                $state.go("public.login");
            });
        },
         function (error) {
             console.log(error);
             if (error.status == -1)
             {  LoginService.showMessage("", error); }
             else
             {
                 swal({
                     title: "Reset password fallito!",
                     text: "La tuo richiesta di reset password non ha avuto successo.<br/>Richiedi un nuovo codice per resettare la password<br/><i>" + error.statusText + "</i>: <small>" + error.data + "</small>",
                     type: "error",
                     html: true
                 }, function () {
                     $state.go("public.login");
                 });
             }
         });
    }

    $scope.confirmEmail = function () {
        swal({
            title: "Verifica il tuo account",
            text: "Verifichiamo che il codice che ci hai mandato corrisponda al tuo account.",
            type: "info",
            closeOnConfirm: false,
            showLoaderOnConfirm: true,
            confirmButtonText: "Ok",
        }, function () {
            var idUser = $stateParams.idUser;
            var code = $stateParams.code;
            LoginService.confirmEmail($stateParams.idUser, $stateParams.code).then(function (result) {
                swal({
                    title: "Account confermato!",
                    text: "<strong>Il tuo account è stato confermato!</strong><br />Accedi direttamente dall'applicazione.",
                    type: "success",
                    html: true
                }, function () {
                    $state.go("public.login");
                });
            },
            function (error) {
                console.log(error);
                if (error.status == -1)
                { LoginService.showMessage("", error); }
                else
                {
                    swal({
                        title: "Conferma account fallita!",
                        text: "Il tuo account non è stato confermato.<br/>Richiedi un nuovo codice di verifica<br/><i>" + error.statusText + "</i>: <small>" + error.data + "</small>",
                        type: "error",
                        html: true
                    }, function () {
                        $state.go("public.login");
                    });
                }
            })
        });
    }

    

})

