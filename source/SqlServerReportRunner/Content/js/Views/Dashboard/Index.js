$(document).ready(function () {
    var vm = new Vue({
        el: '#vue-dashboard-index',
        data: {
            currentConnection: null,
            totalReportCount: 0,
        },
        // define methods under the `methods` object
        methods: {
            // initialisation method, called once the page has loaded
            initialise: function () {
                var connections = $('.link-connection');
                if (connections.length > 0) {
                    $('.link-connection').click(this.onConnectionClick);
                    var link = connections.first();
                    link.addClass('active').click();
                }
            },
            onConnectionClick: function (evt) {
                this.currentConnection = $(evt.currentTarget).data('conn-name');
                this.reloadAll();
            },
            // reloads all data on the dashboard
            reloadAll: function () {
                this.reloadStatistics();
            },
            // reloads all statistics on the page
            reloadStatistics: function () {
                var that = this;
                that.toggleStatistics(false);
                $.ajax({
                    type: "POST",
                    data: { 'ConnName': this.currentConnection },
                    url: '/dashboard/statistics',
                    success: function (data) {
                        that.totalReportCount = data.totalReportCount;
                        that.toggleStatistics(true);
                    },
                    dataType: 'json'
                });
            },
            // method used to toggle the status of the statistics boxes while data is being reloaded
            toggleStatistics: function (isEnabled) {
                $('#panel-statistics').find('.reload').each(function (index, element) {
                    var iconClass = $(this).data('icon');
                    isEnabled ? 
                        $(this).removeClass('fa-refresh').removeClass('fa-spin').addClass(iconClass) :
                        $(this).removeClass(iconClass).addClass('fa-refresh').addClass('fa-spin');
                });
            }
        }
    });
    vm.initialise();
});
