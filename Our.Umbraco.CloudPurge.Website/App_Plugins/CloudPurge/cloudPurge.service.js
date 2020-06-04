angular.module('umbraco').factory('cloudPurgeService',
    function ($http) {


        function prepareResponse(response) {
            response.data.ContentFilter.IncludedContentTypes =
                response.data.ContentFilter.IncludedContentTypes.join(',');
            response.data.ContentFilter.ExcludedContentTypes =
                response.data.ContentFilter.ExcludedContentTypes.join(',');

            return response;
        }

        function prepareRequest(config) {
            var request = _.clone(config);

            function nullOrEmpty(x) {
	            return typeof (x) === "undefined" || x === null || x === "";
            }

            request.ContentFilter.IncludedContentTypes =
	            nullOrEmpty(request.ContentFilter.IncludedContentTypes)
	            ? []
	            : request.ContentFilter.IncludedContentTypes.split(',');

            request.ContentFilter.ExcludedContentTypes =
	            nullOrEmpty(request.ContentFilter.ExcludedContentTypes)
	            ? []
	            : request.ContentFilter.ExcludedContentTypes.split(',');

            return request;
        }

        return {
            getConfig: function () {
                return $http.get('backoffice/CloudPurge/CloudPurgeApi/Config')
                    .then(prepareResponse);
            },

            postConfig: function (config) {
	            var request = prepareRequest(config);
                return $http.post('backoffice/CloudPurge/CloudPurgeApi/Config', request)
                    .then(prepareResponse);
            },

            purgeAll: function () {
                return $http.get('backoffice/CloudPurge/CloudPurgeApi/PurgeAll');
            },

            purgeContent: function (id, descendants) {
                return $http.get(`backoffice/CloudPurge/CloudPurgeApi/Purge/${id}?descendants=${descendants}`);
            }

        }
    });