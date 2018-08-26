$(document).ready(function () {
    var vm = new Vue({
        el: '#vue-dashboard-index',
        data: {
            totalReportCount: 0,
        },
        // define methods under the `methods` object
        methods: {
            initialise: function () {
                this.reloadAll();
            },
            reloadAll: function () {
                $(".reload").each(function (index) {
                    $(this).removeClass('fa-cube').addClass('fa-refresh').addClass('fa-spin');
                });
                this.reloadTotalCount();
            },
            reloadTotalCount: function () {
                this.resetStatBox('#stat-total-report-count', 'fa-cube')
            },
            resetStatBox: function (boxId, iconClass) {
                //debugger;
                $(boxId).find('.reload').addClass(iconClass).removeClass('fa-refresh').removeClass('fa-spin')
            }
        }
    });
    vm.initialise();
});
