angular.module('umbraco').factory('cloudPurgeService',
    function ($http) {

        return {
            getConfig: function () {
                return $http.get('backoffice/CloudPurge/CloudPurgeApi/Config');
            },

            postConfig: function (config) {
                return $http.post('backoffice/CloudPurge/CloudPurgeApi/Config', config);
            },

            purgeAll: function () {
                return $http.get('backoffice/CloudPurge/CloudPurgeApi/PurgeAll');
            },

            purgeContent: function (id, descendants) {
                return $http.get(`backoffice/CloudPurge/CloudPurgeApi/Purge/${id}?descendants=${descendants}`);
            },
        }
    });