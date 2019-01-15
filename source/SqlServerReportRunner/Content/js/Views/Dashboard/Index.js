$(document).ready(function () {
    var vm = new Vue({
        el: '#vue-dashboard-index',
        data: {
            chartMostActiveUsers: null,
            chartMostRunReports: null,
            chartReportCountByDay: null,
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
            reloadChart: function (chart, data) {
                var keys = [];
                var counts = [];
                for (var property in data) {
                    if (data.hasOwnProperty(property)) {
                        keys.push(data[property].key.replace(/\\/g, '\\&#8203;').replace(/\./g, '.&#8203;'));   // replace gets around domain user names that contain \ and . display issues
                        counts.push(Number(data[property].count));
                    }
                }
                var chartData = {
                    labels: keys,
                    series: [
                        counts
                    ]
                };
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

                        that.reloadChart(that.chartMostActiveUsers, data.mostActiveUsers);
                        that.reloadChart(that.chartMostRunReports, data.mostRunReports);
                        that.reloadChart(that.chartReportCountByDay, data.reportCountByDay);

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
            var that = this;
            this.chartMostActiveUsers = new Chartist.Bar('#active-users-chart');
            this.chartMostRunReports = new Chartist.Bar('#active-reports-chart');
            this.chartReportCountByDay = new Chartist.Line('#report-count-by-day-chart', {}, {
                low: 0,
                showArea: true,
                axisX: {
                    labelInterpolationFnc: function (value, index) {
                        // make sure we don't show too many labels
                        var maxLabels = 10;
                        if (that.chartReportCountByDay.data.labels.length <= maxLabels) {
                            return value;
                        }
                        else {
                            var mod = parseInt(that.chartReportCountByDay.data.labels.length / maxLabels);
                            return index % mod === 0 ? value : null;
                        }
                    }
                }
            });

            // initialise the connection
            var connections = $('.link-connection');
            if (connections.length > 0) {
                $('.link-connection').click(this.onConnectionClick);
                $('#txt-date-filter').daterangepicker({
                    //opens: 'right',
                    startDate: this.startDate,
                    endDate: this.endDate,
                    locale: {
                        format: 'YYYY/MM/DD'
                    }
                }, this.onDateFilterChange);
                connections.first().click();
            }
            // initialise tool tips
            $('[data-toggle="tooltip"]').tooltip();
        }
    });
});
