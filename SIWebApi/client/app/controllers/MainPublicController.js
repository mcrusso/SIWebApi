siwebapi.controller('MainPublicController', function ($rootScope, $scope, $state, $location, $timeout, $anchorScroll) {
  $scope.scrollTo = function (id) {
    $scope.currentPosition = id;
    if ($state.current.name == 'public.login')
      customScrollTo(id);
    else
      $state.go("public.login");
  }

  $rootScope.$on('$viewContentLoaded',
      function (event) {
        customScrollTo($scope.currentPosition);
      });

  function customScrollTo(id) {
    $location.hash(id);
    $('html, body').stop().animate({
      scrollTop: ($("#" + id).offset().top - 50)
    }, 1250, 'easeInOutExpo');
    $('.navbar-toggle:visible').click();
  }

  $('body').scrollspy({
    target: '.navbar-fixed-top',
    offset: 100
  });

  $('#mainNav').affix({
    offset: {
      top: 50
    }
  })
});
