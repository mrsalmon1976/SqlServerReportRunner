$(document).ready(function () {
    var vm = new Vue({
        el: '#vue-dashboard-index',
        data: {
            currentConnection: null,
            totalReportCount: 0,
            startDate: moment().subtract(3, 'months'),
            endDate: moment(),
        },
        methods: {
            // initialisation method, called once the page has loaded
            initialise: function () {
                var connections = $('.link-connection');
                if (connections.length > 0) {
                    $('.link-connection').click(this.onConnectionClick);
                    connections.first().click();
                    $('#txt-date-filter').daterangepicker({
                        //opens: 'right',
                        startDate: this.startDate,
                        endDate: this.endDate,
                        locale: {
                            format: 'YYYY/MM/DD'
                        }
                    }, this.onDateFilterChange);
                }
            },
            onConnectionClick: function (evt) {
                var connections = $('.link-connection');
                connections.removeClass('active');
                var ele = $(evt.currentTarget);
                this.currentConnection = ele.data('conn-name');
                ele.addClass('active');
                this.reloadAll();
            },
            onDateFilterChange: function (start, end, label) {
                this.startDate = start;
                this.endDate = end;
                this.reloadAll();
            },
            onUserFilterHide: function (event) {
                console.log("A new user selection was made: " + event);
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
                    data: {
                        'ConnName': this.currentConnection,
                        'StartDate': this.startDate.format('YYYY-MM-DD'),
                        'EndDate': this.endDate.add('days', 1).format('YYYY-MM-DD')
                    },
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
