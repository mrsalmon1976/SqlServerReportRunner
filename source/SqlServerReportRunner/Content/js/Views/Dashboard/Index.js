$(document).ready(function () {
    var vm = new Vue({
        el: '#vue-dashboard-index',
        data: {
            currentConnection: null,
            startDate: moment().subtract(3, 'months'),
            endDate: moment(),
            totalReportCount: 0,
            averageExecutionSeconds: 0,
            averageGenerationSeconds: 0,
        },
        methods: {
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
                        'EndDate': moment(this.endDate).add(1, 'days').format('YYYY-MM-DD')
                    },
                    url: '/dashboard/statistics',
                    success: function (data) {
                        that.totalReportCount = data.totalReportCount;
                        that.averageExecutionSeconds = data.averageExecutionSeconds;
                        that.averageGenerationSeconds = data.averageGenerationSeconds;
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
        },
        mounted: function () {
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
            // initialise tool tips
            $('[data-toggle="tooltip"]').tooltip();

        }
    });
});
