angular.module("umbraco").controller("CloudPurge.DashboardController",
	function ($scope, cloudPurgeService, notificationsService, contentTypeResource) {

		$scope.confirmPurgeNow = false;
		$scope.loading = true;

		cloudPurgeService.getConfig().then(function(response) {
			$scope.config = response.data;
			$scope.loading = false;

		}).catch(function(ex) {
			notificationsService.error('An error occurred while loading the CloudPurge config.')
			console.error(ex);

			$scope.loading = false;
		});

		// #region event handlers

		$scope.save = function () {

			$scope.loading = true;

			cloudPurgeService.postConfig($scope.config).then(function (response) {
				$scope.config = response.data;
				notificationsService.success('Config updated');

				$scope.loading = false;

			}).catch(function (ex) {
				notificationsService.error('An error occurred while saving the CloudPurge config.')
				console.error(ex);

				$scope.loading = false;
			});
		}

		$scope.purgeNow = function () {
			
			$scope.loading = true;

			cloudPurgeService.purgeAll().then(function (response) {
				notificationsService.success('Purged all CDN cache');

				$scope.loading = false;
				$scope.confirmPurgeNow = false;

			}).catch(function (ex) {
				notificationsService.error('An error occurred while purging CloudPurge cache')
				console.error(ex);

				$scope.loading = false;
				$scope.confirmPurgeNow = false;
			});
		}

		$scope.togglePurgeNow = function () {
			$scope.confirmPurgeNow = !$scope.confirmPurgeNow;
		}

		$scope.togglePublishHooks = function () {
			$scope.config.EnablePublishHooks = !$scope.config.EnablePublishHooks;
		}

		$scope.toggleCloudFlareEnabled = function () {
			$scope.config.CloudFlare.Enabled = !$scope.config.CloudFlare.Enabled;
		}
		// #endregion
	});

angular.module("umbraco").controller("CloudPurge.PurgeActionController",
	function ($scope, navigationService, cloudPurgeService) {

		$scope.source = _.clone($scope.currentNode);
		$scope.decendants = false;

		// #region event handlers

		$scope.purgeNow = function () {

			$scope.loading = true;

			cloudPurgeService.purgeContent($scope.source.id, $scope.decendants).then(function (response) {
				$scope.loading = false;
				$scope.result = {
					success: response.data.Success,
					failMessages: response.data.FailMessages || [],
					failedUrls: response.data.FailedUrls || []
				};
				console.log($scope.result);

			}, function (ex) {
				console.log(ex);

				$scope.loading = false;
				$scope.result = {
					success: false,
					failMessages: [ex.errorMsg],
					failedUrls: []
				};

			});
		}

		$scope.toggleDecendants = function () {
			$scope.decendants = !$scope.decendants;
		}

		$scope.closeDialog = function () {
			navigationService.hideDialog();
		}

		// #endregion
	});