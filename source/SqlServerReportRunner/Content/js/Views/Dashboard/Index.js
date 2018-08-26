$(document).ready(function () {
    var vm = new Vue({
        el: '#vue-dashboard-index',
        data: {
            totalReportCount: 0,
        },
        // define methods under the `methods` object
        methods: {
            initialise: function () {
                this.reload();
            },
            reload: function () {
                $(".reload").each(function (index) {
                    //console.log(index + ": " + $(this).text());
                    $(this).removeClass('fa-cube');
                    $(this).addClass('fa-refresh').addClass('fa-spin');
                });
            },
        }
    });
    vm.initialise();
    //$('#txtPresentValue').focus();
});
