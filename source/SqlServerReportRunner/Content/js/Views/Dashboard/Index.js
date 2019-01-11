$(document).ready(function () {
    var vm = new Vue({
        el: '#vue-dashboard-index',
        data: {
            chartMostActiveUsers: null,
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
            reloadActiveUsers: function (data) {
                var users = [];
                var reportCounts = [];
                for (var property in data) {
                    if (data.hasOwnProperty(property)) {
                        users.push(data[property].userName);
                        reportCounts.push(Number(data[property].reportCount));
                    }
                }
                var chart = this.chartMostActiveUsers;
                var chartData = {
                    labels: users,
                    series: [
                        reportCounts
                    ]
                };

                //data2 = {
                //    labels: ['Z1', 'Z2', 'Z3', 'Z4'],
                //    series: [
                //        [Math.random() * 1000, Math.random() * 1000, Math.random() * 1000, Math.random() * 1000]
                //    ]
                //};

                chart.update(chartData);
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
                        that.reloadActiveUsers(data.mostActiveUsers);
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
            // initialise charts
            this.chartMostActiveUsers = new Chartist.Bar('#active-users-chart');
            // initialise the connection
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
