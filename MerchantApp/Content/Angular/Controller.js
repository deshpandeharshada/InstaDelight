
app.controller("myCntrl", function ($scope, $rootScope, angularService) {
    $rootScope.searchButtonText = false;

    $scope.locationmodel = [];
    $scope.locationsettings = {
        scrollableHeight: '150px',
        scrollable: true,
        enableSearch: false
    };

    $scope.selectedAll = function () {
        $scope.groups.forEach(function (x) {
            if ($scope.selectAll) {
                x.selected = true;
            }
            else {
                x.selected = false;
            }
        });
    }


    $rootScope.showFooter = true;
    $rootScope.languagecode = "en";

    $scope.locationdata = null;
    $scope.currency = "";

    $scope.CountryList = null;
    $scope.StateList = null;
    $scope.LocationList = null;
    $scope.CityList = null;
    $scope.CategoryList = null;
    $scope.CouponList = null;

    $scope.coupondecdata = null;
    $scope.couponqrcodedata = null;
    $scope.merchantlogodata = null;
    $scope.merchantdecdata = null;
    $scope.SelectedImageSrc = null;

    $scope.denom1decdata = null;
    $scope.denom2decdata = null;
    $scope.denom3decdata = null;
    $scope.denom4decdata = null;

    $scope.divCoupon = false;
    $scope.divCoupons = true;

    $scope.divProfile = true;

    $scope.locationid = 0;
    $scope.cityid = 0;
    $scope.countryid = 0;
    $scope.stateid = 0;
    $scope.expid = 0;
    $scope.bcouponid = 0;
    $scope.acouponid = 0;
    $scope.rcouponid = 0;
    $scope.scouponid = 0;

    //for terms and condition
    var counter = 0;

    $scope.lblCountryCode = "";

    $rootScope.divRedeemCoupon = true;
    $rootScope.divConsumerProfile = false;
    $rootScope.divRedeemPoints = false;
    $rootScope.divCoupons = false;
    $rootScope.divShowMessage = false;

    $scope.validitymsg = "";

    $scope.isCheckboxSelected = function (index) {

        return index === $scope.checkboxSelection;
    };

    $scope.isRedeemSelected = function (index) {

        return index === $scope.RedeemSelection;
    };

    //$scope.IsReward = false;
    //$scope.ShowHide = function () {

    //    $scope.IsReward = $scope.rewardSelection;
    //}

    $scope.numRows = 5;
    $scope.gridOptions = {
        columnDefs: [
         { name: 'CouponTitle', headerCellTemplate: '<div></div>', cellClass: 'htabGrid', width: "*", resizable: false, cellTemplate: '<div ng-click="grid.appScope.editCoupon(row.entity)">{{row.entity.CouponTitle}}</div>' },
          { name: 'couponid', cellTemplate: '<a href="#">{{row.entity.couponid}}</a>', headerCellTemplate: '<div></div>', visible: false },
          { name: 'Share', cellClass: 'htabGridshare small-2 columns', cellTemplate: ' <div style="float:right;margin-right: 4px;"> <a href="/Merchant/SendCoupon?CouponId={{row.entity.couponid}}"><i class="fa fa-share-alt fa-2x red-icon" aria-hidden="true"></i></a>  </div>', headerCellTemplate: '<div></div>', width: "15%", resizable: false }
        ],
        enableColumnResize: false,
        enableExpandable: false,
        rowHeight: 65,
        paginationPageSize: $scope.numRows,
        paginationPageSizes: [5, 10, 50, 100],
        enablePaginationControls: true,
        paginationCurrentPage: 1,
    };

    $scope.gridOptions.onRegisterApi = function (gridApi) {
        $scope.gridApi2 = gridApi;
    }

    $scope.gridCustOptions = {
        columnDefs: [
         { name: 'CouponTitle', headerCellTemplate: '<div></div>', cellClass: 'htabGrid', width: "*", resizable: false, cellTemplate: '<div>{{row.entity.CouponTitle}}</div>' },
          { name: 'Share', cellClass: 'htabGridshare small-3 columns', cellTemplate: '  <a href="/Merchant/VerifyAndRedeemCoupon?CouponId={{row.entity.couponid}}&SharedCouponId={{row.entity.Id}}">Redeem</a>  ', headerCellTemplate: '<div></div>', width: "20%", resizable: false }
        ],
        enableColumnResize: false,
        enableExpandable: false,
        rowHeight: 65,
        paginationPageSize: $scope.numRows,
        paginationPageSizes: [5, 10, 50, 100],
        enablePaginationControls: true,
        paginationCurrentPage: 1,
    };

    $scope.gridCustOptions.onRegisterApi = function (gridApi) {
        $scope.gridApi2 = gridApi;
    }

    $scope.gridGiftOptions = {
        columnDefs: [
         { name: 'CardTitle', headerCellTemplate: '<div></div>', cellClass: 'htabGrid', width: "*", resizable: false, cellTemplate: '<div>{{row.entity.DenominationRs}}</div>' },
          { name: 'Share', cellClass: 'htabGridshare small-3 columns', cellTemplate: '  <a href="/Merchant/VerifyAndRedeemGiftCard?Id={{row.entity.Id}}">Redeem</a>  ', headerCellTemplate: '<div></div>', width: "20%", resizable: false }
        ],
        enableColumnResize: false,
        enableExpandable: false,
        rowHeight: 65,
        paginationPageSize: $scope.numRows,
        paginationPageSizes: [5, 10, 50, 100],
        enablePaginationControls: true,
        paginationCurrentPage: 1,
    };

    $scope.gridGiftOptions.onRegisterApi = function (gridApi) {
        $scope.gridApi4 = gridApi;
    }

    $scope.gridCust = {
        columnDefs: [
         { name: 'CustomerPhone', headerCellTemplate: '<div></div>', cellClass: 'htabGrid', width: "*", resizable: false, cellTemplate: '<a href="/Merchant/CustomerSummaryReport?ConsumerId={{row.entity.ConsumerId}}">{{row.entity.ConsumerPhone}}</a>' },
        ],
        enableColumnResize: false,
        enableExpandable: false,
        rowHeight: 65,
        paginationPageSize: $scope.numRows,
        paginationPageSizes: [5, 10, 50, 100],
        enablePaginationControls: true,
        paginationCurrentPage: 1,
    };

    $scope.gridCust.onRegisterApi = function (gridApi) {
        $scope.gridApi3 = gridApi;
    }

    $scope.gridBrands = {
        columnDefs: [
         { name: 'BrandName', cellClass: 'htabGrid', headerCellTemplate: '<div></div>', width: "*", resizable: false, cellTemplate: '<div ng-click="grid.appScope.editBrand(row.entity)">{{row.entity.BrandName}}</div>' },
          //{ name: 'AddLocation', cellClass: 'htabGridshare small-2 columns', headerCellTemplate: '<div>Add Location</div>', cellTemplate: '<div style="text-align:center"> <a ng-click="grid.appScope.AddLocation(row.entity)"><i class="fa fa-plus fa-2x red-icon" aria-hidden="true"></i></a>  </div>', width: "25%", resizable: false },
          { name: 'Delete', cellClass: 'htabGridDelete small-2 columns', headerCellTemplate: '<div></div>', cellTemplate: ' <div style="text-align:center"> <i class="fa fa-remove fa-2x red-icon" aria-hidden="true"></i></div>', width: "20%", resizable: false }
        ],
        enableColumnResize: false,
        enableExpandable: true,
        rowHeight: 65,
        showHeader: false,
        paginationPageSize: $scope.numRows,
        paginationPageSizes: [5, 10, 50, 100],
        enablePaginationControls: true,
        paginationCurrentPage: 1,
        expandableRowTemplate: '<div style="height:100px;padding:0px;overflow-y:auto;"><div ui-grid="row.entity.subGridOptions" style="{border:none;}"></div></div>',
        expandableRowHeight: 140,
        //subGridVariable will be available in subGrid scope
        expandableRowScope: {
            clickMeSub: function (row) { editLocation(row.entity); }
        }
    };

    $scope.gridBrands.onRegisterApi = function (gridApi) {
        $scope.gridApi2 = gridApi;
        //$scope.gridApi2.expandable.on.rowExpandedStateChanged($scope, function (row) {
        //    $timeout(function () {
        //        row.expandedRowHeight = row.entity.youList.length * row.entity.subGridOptions.rowHeight + 39;
        //    }, 150);
        //});
    }

    $scope.gridUsers = {
        columnDefs: [
         { name: 'StaffName', cellClass: 'htabStaffGrid', headerCellTemplate: '<div></div>', width: "*", resizable: false, cellTemplate: '<div ng-click="grid.appScope.editStaff(row.entity)">{{row.entity.StaffName}}</div>' },
          { name: 'Delete', cellClass: 'htabGridDelete small-2 columns', headerCellTemplate: '<div></div>', cellTemplate: ' <div style="text-align:center"><i class="fa fa-remove fa-2x red-icon" aria-hidden="true"></i>  </div>', width: "40%", resizable: false }
        ],
        enableColumnResize: false,
        rowHeight: 65,
        showHeader: false,
        enableExpandable: false,
        paginationPageSize: $scope.numRows,
        paginationPageSizes: [5, 10, 50, 100],
        enablePaginationControls: true,
        paginationCurrentPage: 1
    };

    $scope.gridUsers.onRegisterApi = function (gridApi) {
        $scope.gridApi3 = gridApi;
    }

    $scope.setLanguageCode = function () {
        getLanguageCode();
    }



    $scope.openManageLicenses = function () {

        $scope.divBrands = true;
        $scope.divAddBrand = false;
        $scope.divAddLocation = false;
        $scope.divAddStaff = false;
        getLanguageCode();
        fillBusinessCategoryList();
        fillBrandList();
        getLicenses();
        var countryId = document.getElementsByName('hdnCountry')[0].value;
        if (countryId != undefined) {
            if (countryId != "0") {
                {
                    fillCountry();

                    var getCountryData = angularService.getCountrycode();
                    getCountryData.then(function (mch) {
                        $scope.selectedcountrycodeobject = { countryid: countryId, CountryCode: mch.data.CountryCode };
                    }, function (err) {
                        if (err != null) {
                            if (err.data != null) {
                                alert(err.data);
                                alert('Error in getting records openManageLicenses');
                            }
                        }

                    });
                }
            }
        }


    }

    $scope.OpenForm = function () {
        getLanguageCode();
        //fillCityList();        
        getCurrency();
        //fillLocationListForCoupon();
        fillBusinessCategoryList();
        GetCountryCode();
    }

    $scope.OpenCouponList = function () {
        if (document.getElementsByName('hdnSharingAllowed')[0].value == "false") {
            $scope.gridOptions.columnDefs[2].visible = false;
        }

        getLanguageCode();
        //fillCityList();        
        getCurrency();
        //fillLocationListForCoupon();
        fillBusinessCategoryList();
        GetAllCoupons();
        GetCountryCode();
    }

    $scope.OpenCustomerList = function () {
        var getData = angularService.getCustomers();

        getData.then(function (mch) {
            $scope.gridCust.excessRows = mch.data.length;
            $scope.gridCust.data = mch.data;
            $scope.gridCust.totalItems = mch.data.length;
            if (mch.data.length < $scope.numRows) {
                $scope.gridCust.minRowsToShow = mch.data.length;
            }
            else {
                $scope.gridCust.minRowsToShow = $scope.numRows;
            }

        }, function (mcherror) {
            if (mcherror.data != null) {
                alert(mcherror.data);
                alert('Error in getting records OpenCustomerList');
            }
        });
    }

    $scope.GetGroups = function () {
        getLanguageCode();
        GetAllGroups();
    }


    $scope.DisplayGiftCard = function () {
        var OTPStatus = document.getElementsByName('hdnOTP')[0].value;
        if (OTPStatus == "OTP Sent Successfully.") {
            $scope.divShowOTP = true;
            $scope.divOTPSent = true;
            $scope.OTPError = false;
        }
        else {
            $scope.divShowOTP = false;
            $scope.divOTPSent = true;
            $scope.OTPError = true;
        }

    }

    $scope.VerifyOTP = function () {

        if ($scope.otp != "") {
            var GiftcardId = document.getElementsByName('hdnId')[0].value;

            var getData = angularService.VerifyOTP($scope.otp, GiftcardId);

            getData.then(function (msg) {

                var OTPVerified = angularService.getString("OTPVerified", $rootScope.languagecode);
                if (msg.data == OTPVerified) {
                    alert(msg.data);
                    $scope.divShowOTP = false;
                    $scope.divOTPSent = false;
                    $scope.OTPError = false;
                }
                else {
                    alert(msg.data);
                    $scope.divShowOTP = false;
                    $scope.divOTPSent = true;
                    $scope.OTPError = false;
                }
            }, function (err) {
                $scope.divShowOTP = false;
                $scope.divOTPSent = true;
                $scope.OTPError = false;
                if (err != null) {
                    if (err.data != null) {
                        alert(err.data);
                        alert('Error in VerifyEmailOTP');
                    }
                }
            });
        }
    }

    $scope.DisplayCoupon = function () {
        getLanguageCode();
        var CouponId = document.getElementsByName('hdnCouponId')[0].value;

        var getData = angularService.getCoupon(CouponId);

        getData.then(function (cpn) {

            $scope.categoryid = cpn.data.categoryid;
            $scope.coupontitle = cpn.data.CouponTitle;
            $scope.couponcode = cpn.data.CouponCode;
            $scope.MerchantId = cpn.data.MerchantId;
            $scope.divShowCouponOTP = false;
            $scope.divVerifiedCouponOTP = false;
            $scope.EventConditions = null;

            var getEventConditions = angularService.getEventConditions(cpn.data.couponid);
            getEventConditions.then(function (condition) {

                if (condition.data.length != 0) {
                    $scope.EventConditions = condition.data;
                }
            },
            function (errdata) {

                if (errdata != null) {
                    if (errdata.data != null) {
                        alert(errdata.data);
                        $rootScope.searchButtonText = false;
                        alert("Error in getting event condition");
                    }
                }
            });

            $scope.CouponConditions = cpn.data.conditions;

            if (cpn.data.ValidTill != null) {
                $scope.validtill = cpn.data.ValidTill;
                $scope.validtill = $scope.validtill.replace('/Date(', '');
                $scope.validtill = $scope.validtill.replace(')/', '');
                $scope.validtill = new Date(parseInt($scope.validtill)).toDateString();
            }

            if (cpn.data.DEC != null) {
                var CHUNK_SIZE = 0x8000; // arbitrary number here, not too small, not too big
                var index = 0;
                var length = cpn.data.DEC.length;
                var result = '';
                var slice;
                while (index < length) {
                    slice = cpn.data.DEC.slice(index, Math.min(index + CHUNK_SIZE, length)); // `Math.min` is not really necessary here I think
                    result += String.fromCharCode.apply(null, slice);
                    index += CHUNK_SIZE;
                }


                $scope.b64decencoded = result;//String.fromCharCode.apply(null, coupon.DEC);
                $scope.coupondecdata = btoa($scope.b64decencoded);
            }


            $scope.coupondetails = cpn.data.CouponDetails;

            var getMerchantDetails = angularService.getMerchant(cpn.data.MerchantId);
            getMerchantDetails.then(function (merchant) {
                $scope.merchant = merchant.data;
            },
            function (errdata) {
                alert(errdata.data);
                $rootScope.searchButtonText = false;
                alert("Error in getting merchant details");
            });
        },
                function () {
                    alert(errdata.data);
                    $rootScope.searchButtonText = false;
                    alert("Error in getting coupon details");
                });

    }

    $scope.OpenHomeScreen = function () {
        getLanguageCode();
        $rootScope.showFooter = false;
    }

    $scope.OpenReportScreen = function () {
        getCurrency();
        $rootScope.showFooter = true;
    }

    $scope.EnableHomeBackButton = function () {
        $rootScope.showFooter = true;
    }

    $scope.OpenReviewForm = function () {
        getLanguageCode();
        $rootScope.showFooter = true;
        GetRecentReview();
    }

    function getLanguageCode() {
        var getData = angularService.getLanguageCode();
        getData.then(function (lang) {
            $rootScope.languagecode = lang.data;
        },
           function (errdata) {
               if (errdata != null) {
                   if (errdata.data != null) {
                       alert(errdata.data);
                       alert("Error in getting language details");
                   }
               }
           });
    }

    $scope.OpenRedeemForm = function () {
        var countryId = document.getElementsByName('hdnCountry')[0].value;
        if (countryId != undefined) {
            if (countryId != "0") {
                {
                    fillCountry();

                    var getCountryData = angularService.getCountrycode();
                    getCountryData.then(function (mch) {
                        $scope.selectedcountrycodeobject = { countryid: countryId, CountryCode: mch.data.CountryCode };
                    }, function (err) {
                        if (err != null) {
                            if (err.data != null) {
                                alert(err.data);
                                alert('Error in getting records OpenRedeemForm');
                            }
                        }

                    });
                }
            }
        }
        getCurrency();
        $rootScope.divRedeemCoupon = true;
        $rootScope.divConsumerProfile = false;
        $rootScope.divRedeemPoints = false;
        $rootScope.divShowMessage = false;
        $rootScope.searchButtonText = false;
    }

    $scope.OpenSendDECForm = function () {
        getLanguageCode();
        var countryId = document.getElementsByName('hdnCountry')[0].value;
        if (countryId != undefined) {
            if (countryId != "0") {
                {
                    fillCountry();

                    var getCountryData = angularService.getCountrycode();
                    getCountryData.then(function (mch) {
                        $scope.selectedcountrycodeobject = { countryid: countryId, CountryCode: mch.data.CountryCode };
                    }, function (err) {
                        if (err != null) {
                            if (err.data != null) {
                                alert(err.data);
                                alert('Error in getting records OpenSendDECForm');
                            }
                        }

                    });
                }
            }
        }
    }

    $scope.sendSelectedGroups = function () {
        $scope.selectedGroupNames = "";

        var selectcount = 0;
        for (var k = 0; k < $scope.groups.length; k++) {
            if ($scope.groups[k].selected == true) {
                //Perform your desired thing over here
                $scope.selectedGroupNames = $scope.groups[k].GroupName + ",";
            }
        }

        if ($scope.selectedGroupNames != "") {
            var couponid = document.getElementsByName('hdnCouponId')[0].value;

            var getData = angularService.sendCouponToGroups($scope.selectedGroupNames, couponid);
            getData.then(function (mch) {
                alert(mch.data);
            }, function (errdata) {
                if (errdata != null) {
                    if (errdata.data != null) {
                        alert(errdata.data);
                        alert('Error in getting records sendSelectedGroups');
                    }
                }
            });
        }
    }

    function GetRecentReview() {
        var getData = angularService.getReview();
        getData.then(function (rev) {
            $scope.review = rev.data;
            $scope.reviewid = rev.data.reviewid;
            $scope.Question1 = rev.data.Question1;
            $scope.Question2 = rev.data.Question2;
            $scope.Question3 = rev.data.Question3;
            $scope.Question4 = rev.data.Question4;
        }, function (errdata) {
            alert(errdata.data);
            alert('Error in getting records GetRecentReview');
        });
    }

    $scope.OpenGiftsForm = function () {
        //for terms and condition---------------------------------------
        getLanguageCode();
        $rootScope.showFooter = true;
        $scope.conditionelemnt = [];
        getCurrency();
        GetRecentGifts();
    }



    function GetRecentGifts() {
        var getData = angularService.getGifts();
        getData.then(function (gft) {
            $scope.giftcard = gft.data;
            $scope.giftcardid = gft.data.GiftCardId;
            $scope.Denomination1 = gft.data.Denomination1;
            if (gft.data.Denom1DEC != null) {
                var CHUNK_SIZE = 0x8000; // arbitrary number here, not too small, not too big
                var index = 0;
                var length = gft.data.Denom1DEC.length;
                var result = '';
                var slice;
                while (index < length) {
                    slice = gft.data.Denom1DEC.slice(index, Math.min(index + CHUNK_SIZE, length)); // `Math.min` is not really necessary here I think
                    result += String.fromCharCode.apply(null, slice);
                    index += CHUNK_SIZE;
                }


                $scope.denom1encoded = result;//String.fromCharCode.apply(null, cpn.data.DEC);
                $scope.denom1decdata = btoa($scope.denom1encoded);
            }

            $scope.Denomination2 = gft.data.Denomination2;
            if (gft.data.Denom2DEC != null) {
                var CHUNK_SIZE = 0x8000; // arbitrary number here, not too small, not too big
                var index = 0;
                var length = gft.data.Denom2DEC.length;
                var result = '';
                var slice;
                while (index < length) {
                    slice = gft.data.Denom2DEC.slice(index, Math.min(index + CHUNK_SIZE, length)); // `Math.min` is not really necessary here I think
                    result += String.fromCharCode.apply(null, slice);
                    index += CHUNK_SIZE;
                }


                $scope.denom2encoded = result;//String.fromCharCode.apply(null, cpn.data.DEC);
                $scope.denom2decdata = btoa($scope.denom2encoded);
            }

            $scope.Denomination3 = gft.data.Denomination3;
            if (gft.data.Denom3DEC != null) {
                var CHUNK_SIZE = 0x8000; // arbitrary number here, not too small, not too big
                var index = 0;
                var length = gft.data.Denom3DEC.length;
                var result = '';
                var slice;
                while (index < length) {
                    slice = gft.data.Denom3DEC.slice(index, Math.min(index + CHUNK_SIZE, length)); // `Math.min` is not really necessary here I think
                    result += String.fromCharCode.apply(null, slice);
                    index += CHUNK_SIZE;
                }


                $scope.denom3encoded = result;//String.fromCharCode.apply(null, cpn.data.DEC);
                $scope.denom3decdata = btoa($scope.denom3encoded);
            }

            $scope.Denomination4 = gft.data.Denomination4;
            if (gft.data.Denom4DEC != null) {
                var CHUNK_SIZE = 0x8000; // arbitrary number here, not too small, not too big
                var index = 0;
                var length = gft.data.Denom4DEC.length;
                var result = '';
                var slice;
                while (index < length) {
                    slice = gft.data.Denom4DEC.slice(index, Math.min(index + CHUNK_SIZE, length)); // `Math.min` is not really necessary here I think
                    result += String.fromCharCode.apply(null, slice);
                    index += CHUNK_SIZE;
                }


                $scope.denom4encoded = result;//String.fromCharCode.apply(null, cpn.data.DEC);
                $scope.denom4decdata = btoa($scope.denom4encoded);
            }

            $scope.Email = gft.data.Email;

            var conditiondata = angularService.GetGiftCondition(gft.data.GiftCardId);
            conditiondata.then(function (cnd) {
                conditionelemnt = cnd.data;
            });
        }, function () {
            alert('Error in getting records GetRecentGifts');
        });
    }

    //Sending Coupons to Consumer
    $scope.SendCoupontoConsumer = function (form) {
        getLanguageCode();

        if ($scope[form].$valid) {
            if ($scope.MobileNo == undefined && $scope.Email == undefined && $scope.shareondec == false) {
                var text = angularService.getString("MobileNoEmailPrompt", $rootScope.languagecode);
                alert(text);
            }
            else if ($scope.MobileNo == "" && $scope.Email == "" && $scope.shareondec == false) {
                var text = angularService.getString("MobileNoEmailPrompt", $rootScope.languagecode);
                alert(text);
            }
            else if ($scope.MobileNo == undefined && $scope.Email == "" && $scope.shareondec == false) {
                var text = angularService.getString("MobileNoEmailPrompt", $rootScope.languagecode);
                alert(text);
            }
            else if ($scope.MobileNo == "" && $scope.Email == undefined && $scope.shareondec == false) {
                var text = angularService.getString("MobileNoEmailPrompt", $rootScope.languagecode);
                alert(text);
            }
            else {
                var MobileNo = "";
                var ccode = $scope.selectedcountrycodeobject;

                if ($scope.MobileNo != undefined) {
                    if ($scope.MobileNo != "") {
                        MobileNo = ccode.CountryCode + " " + $scope.MobileNo;
                    }
                }

                if (MobileNo == "") {
                    if ($scope.Email != undefined) {
                        if ($scope.Email != "") {
                            MobileNo = $scope.Email;
                        }
                    }
                }


                var CouponId = document.getElementsByName('hdnCouponId')[0].value;
                if ($scope.shareondec) {
                    var getData = angularService.SendToAll(CouponId);
                    getData.then(function (msg) {
                        alert(msg.data);
                        window.location.href = '/Merchant/CouponList';
                    }, function (err) {
                        if (err != null) {
                            if (err.data != null) {
                                alert(err.data);
                                alert('Error in Sending Coupon to All DECs');
                            }
                        }
                    });
                }
                else {
                    var getData = angularService.AddCheckCouponConsumer(MobileNo, CouponId);
                    getData.then(function (msg) {
                        if (String(msg.data).indexOf("Navigation") >= 0) {
                            //alert(msg.data);
                            window.location = msg.data;
                        }
                        else {
                            alert(msg.data);
                            window.location.href = '/Merchant/CouponList';
                        }
                    }, function (err) {
                        if (err != null) {
                            if (err.data != null) {
                                alert(err.data);

                                alert('Error in Sending Coupon to Mobile No');
                            }
                        }
                    });
                }
            }

        } else {
            $scope.showMsgs = true;
        }
    }



    function GetCountryCode() {

        var getData = angularService.getCountrycode();
        getData.then(function (cty) {
            // $scope.countrycode = cty.data;
            $scope.lblCountryCode = cty.data;
        }, function (err) {
            if (err != null) {
                if (err.data != null) {
                    alert('Error in getting records');
                }
            }
        });
    }



    $scope.newItem = function ($event) {
        counter++;
        $scope.conditionelemnt.push({ id: counter, condition: $scope.TermsCondition });
        //  $event.preventDefault();
    }

    $scope.showitems = function ($event) {
        $('#displayitems').css('visibility', 'none');
    }
    //---------------------------------------------------------------

    $scope.OpenCountryForm = function () {
        getLanguageCode();
        $rootScope.showFooter = true;
        fillCountry();

        var countryId = document.getElementsByName('hdnCountryId')[0].value;
        var langId = document.getElementsByName('hdnLangId')[0].value;
        if (countryId != "0") {
            $scope.selectedcountryobject = { countryid: countryId };
            getLanguage($scope.selectedcountryobject);
            $scope.selectedangobject = { LanguageiId: langId };
            $scope.IsCountrySelected = true;
        }

    }

    function fillExpPerionList() {
        var getData = angularService.getExpPeriod();
        getData.then(function (exp) {
            $scope.ExpiryList = exp.data;
        }, function (err) {
            if (err != null) {
                if (err.data != null) {
                    alert('Error in getting records ' + err.data);
                }
            }

        });
    }


    function fillCountry() {

        var getData = angularService.getCountries();
        getData.then(function (cty) {
            $scope.CountryList = cty.data;
        }, function () {
            alert('Error in getting records');
        });
    }

    $scope.getCountryId = function (country) {
        if (country != null) {
            $scope.lblCountryId = country.countryid;
        }
    }

    $scope.SaveChangedCountry = function () {
        var countryid = $scope.lblCountryId;
        var langid = $scope.languageid;
        var getData = angularService.SaveChangedCountry(countryid, langid);
        getData.then(function (msg) {
            alert(msg.data);
            getLanguageCode();
            window.location.href = '/Home/Index';
        }, function () {
            alert('Error in Saving Country');
        });

    };

    $scope.LanguageChanged = function (loc) {
        if (loc != null) {
            $scope.languageid = loc.LanguageiId;
        }
    }

    function showDEC() {
        $scope.divProfile = false;
        $scope.divRewardProgram = false;
        $scope.divImageGallery = false;
        $scope.divEnlargeImage = false;
        $scope.divDEC = true;
    }

    function showRewardProgram() {
        $scope.divProfile = false;
        $scope.divImageGallery = false;
        $scope.divEnlargeImage = false;
        $scope.divDEC = false;
        $scope.divRewardProgram = true;
    }

    $scope.ShowImageGallery = function () {
        $scope.divProfile = false;
        $scope.divDEC = false;
        $scope.divRewardProgram = false;
        $scope.divEnlargeImage = false;
        $scope.divImageGallery = true;
    }

    $scope.CloseImageGallery = function () {
        $scope.divProfile = false;
        $scope.divDEC = true;
        $scope.divRewardProgram = false;
        $scope.divImageGallery = false;
        $scope.divEnlargeImage = false;
    }

    $scope.EnlargeImage = function (src) {
        $scope.SelectedImageSrc = src;
        $scope.divProfile = false;
        $scope.divDEC = false;
        $scope.divRewardProgram = false;
        $scope.divImageGallery = false;
        $scope.divEnlargeImage = true;
    }

    $scope.CloseEnlargedImage = function () {
        $scope.SelectedImageSrc = null;
        $scope.divProfile = false;
        $scope.divDEC = false;
        $scope.divRewardProgram = false;
        $scope.divImageGallery = true;
        $scope.divEnlargeImage = false;
    }

    $scope.SetImage = function (src) {

        $scope.divProfile = false;

        $scope.merchantdecdata = null;
        $scope.divImageGallery = false;
        $scope.divEnlargeImage = false;

        $scope.SelectedImageSrc = src;
        $scope.divDEC = true;
        $scope.divRewardProgram = false;
    }

    //Get Language
    function getLanguage(country) {

        if (country != null) {
            $scope.countryid = country.countryid;
            $scope.lblCountryId = country.countryid;
            var getData = angularService.getLanguage(country.countryid);
            getData.then(function (loc) {
                $scope.LanguageList = loc.data;
            }, function () {
                alert('Error in getting records');
            });
        } else {
            $scope.languageid = 0;
            $scope.LanguageList = null;
        }
    }

    $scope.openseteventsform = function () {
        getLanguageCode();
        GetAllCoupons();
        GetEventsCoupons();
    }

    $scope.OpenProfileForm = function () {
        var flag = document.getElementsByName('hdnFlag')[0].value;
        if (flag == "showDEC") {
            showDEC();
        }
        else if (flag == "showRewardProgram") {
            showRewardProgram();
        }
        getLanguageCode();
        fillCountry();
        getCurrency();
        fillBusinessCategoryList();
        fillExpPerionList();
        editMerchant();
    }

    function fillBusinessCategoryList() {
        var getData = angularService.getBusinessCategory();
        getData.then(function (busi) {
            $scope.CategoryList = busi.data;
        }, function (caterr) {
            if (caterr.data != null) {
                alert('Error in getting records fillBusinessCategoryList');
            }
        });
    }

    function fillBrandList() {
        var getData = angularService.getBrands();
        getData.then(function (busi) {
            $scope.BrandList = busi.data;
        }, function (caterr) {
            if (caterr.data != null) {
                alert('Error in getting records fillBrandList');
            }
        });
    }

    function getCurrency() {
        var getData = angularService.getCurrency();
        getData.then(function (curr) {
            $scope.currency = curr.data;
        }, function (caterr) {
            if (caterr.data != null) {
                alert(caterr.data)
                alert('Error in getting records getCurrency');
            }
        });
    }


    $scope.SendDECtoConsumerList = function () {

        angularService.SendDECtoConsumerList();
    }

    //Sending DEC to Consumer
    $scope.SendDECtoConsumer = function (form) {
        getLanguageCode();

        if ($scope[form].$valid) {
            if ($scope.MobileNo == undefined && $scope.Email == undefined) {
                var text = angularService.getString("MobileNoEmailPrompt", $rootScope.languagecode);
                alert(text);
            }
            else if ($scope.MobileNo == "" && $scope.Email == "") {
                var text = angularService.getString("MobileNoEmailPrompt", $rootScope.languagecode);
                alert(text);
            }
            else if ($scope.MobileNo == undefined && $scope.Email == "") {
                var text = angularService.getString("MobileNoEmailPrompt", $rootScope.languagecode);
                alert(text);
            }
            else if ($scope.MobileNo == "" && $scope.Email == undefined) {
                var text = angularService.getString("MobileNoEmailPrompt", $rootScope.languagecode);
                alert(text);
            }
                //else if ($scope.BillAmount == undefined) {
                //    var text = angularService.getString("BillAmountPrompt", $rootScope.languagecode);
                //    alert(text);
                //}
                //else if ($scope.BillAmount == "") {
                //    var text = angularService.getString("BillAmountPrompt", $rootScope.languagecode);
                //    alert(text);
                //}
            else {
                var MobileNo = "";
                var ccode = $scope.selectedcountrycodeobject;

                if ($scope.MobileNo != undefined) {
                    if ($scope.MobileNo != "") {
                        MobileNo = ccode.CountryCode + " " + $scope.MobileNo;
                    }
                }

                if (MobileNo == "") {
                    if ($scope.Email != undefined) {
                        if ($scope.Email != "") {
                            MobileNo = $scope.Email;
                        }
                    }
                }

                var BillAmt = $scope.BillAmount;

                var getData = angularService.AddCheckDECConsumer(MobileNo, BillAmt);
                getData.then(function (msg) {

                    if (String(msg.data).indexOf("Navigation") >= 0) {
                        //alert(msg.data);
                        window.location = msg.data;
                    }
                    else {
                        alert(msg.data);
                        window.location.href = '/Home/Index';
                    }
                }, function (err) {
                    if (err != null) {
                        if (err.data != null) {
                            alert(err.data);
                            alert('Error in Sending DEC to Mobile No');
                        }
                    }

                });
            }

        } else {
            $scope.showMsgs = true;
        }
    }

    $scope.redeemCoupon = function (couponcode) {

        if (couponcode != undefined) {

            var getCouponData = angularService.GetCouponFromCouponCode(couponcode);
            getCouponData.then(function (coupon) {

                var getData = angularService.RedeemCoupon(coupon.data[0]);
                getData.then(function (msg) {

                    $scope.couponcode = "";
                }, function () {
                    alert('Error in redeem coupon');
                });
            }, function (cpnerror) {
                if (cpnerror.data != null) {
                    alert(cpnerror.data);
                    alert('Not a valid coupon');
                }
            });
        }
        else {
            var text = angularService.getString("CouponCodePrompt", $rootScope.languagecode);
            alert(text);
        }
    }

    $scope.getImageUrl = function (marlogo) {

        var CHUNK_SIZE = 0x8000; // arbitrary number here, not too small, not too big
        var index = 0;
        var length = marlogo.length;
        var result = '';
        var slice;
        while (index < length) {
            slice = marlogo.slice(index, Math.min(index + CHUNK_SIZE, length)); // `Math.min` is not really necessary here I think
            result += String.fromCharCode.apply(null, slice);
            index += CHUNK_SIZE;
        }

        return result;//String.fromCharCode.apply(null, marlogo);
    }

    //To get all group names    
    function GetAllGroups() {
        var getData = angularService.getGroups();

        getData.then(function (grp) {

            $scope.groups = grp.data;
        }, function (mcherror) {
            if (mcherror.data != null) {
                alert(mcherror.data);
                alert('Error in getting records getGroups');
            }
        });
    }

    //To Get All Records  
    function GetAllCoupons() {

        $scope.divCoupon = false;
        $scope.divCoupons = true;

        $scope.loaderMore = true;
        $scope.lblMessage = 'loading please wait....!';
        $scope.result = "color-green";

        var getData = angularService.getCoupons();

        getData.then(function (mch) {

            $scope.coupons = mch.data;
            $scope.CouponList = mch.data;

            $scope.gridOptions.excessRows = mch.data.length;
            $scope.gridOptions.data = mch.data;
            $scope.gridOptions.totalItems = mch.data.length;
            if (mch.data.length < $scope.numRows) {
                $scope.gridOptions.minRowsToShow = mch.data.length;
            }
            else {
                $scope.gridOptions.minRowsToShow = $scope.numRows;
            }

        }, function (mcherror) {
            if (mcherror.data != null) {
                alert(mcherror.data);
                alert('Error in getting records GetAllCoupons');
            }
        });
    }

    function GetEventsCoupons() {
        var getData = angularService.getEventsCoupons();

        getData.then(function (evtcpns) {


            for (i = 0; i < evtcpns.data.length; i++) {
                if (evtcpns.data[i].EventId == 1) {
                    $scope.bcouponid = evtcpns.data[i].CouponId;
                    $scope.selectedbcouponobject = { couponid: $scope.bcouponid };
                }
                else if (evtcpns.data[i].EventId == 2) {
                    $scope.acouponid = evtcpns.data[i].CouponId;
                    $scope.selectedAcouponobject = { couponid: $scope.acouponid };
                }
                else if (evtcpns.data[i].EventId == 3) {
                    $scope.rcouponid = evtcpns.data[i].CouponId;
                    $scope.selectedRcouponobject = { couponid: $scope.rcouponid };
                }
                else if (evtcpns.data[i].EventId == 4) {
                    $scope.scouponid = evtcpns.data[i].CouponId;
                    $scope.selectedScouponobject = { couponid: $scope.scouponid };
                }

            }
        }, function (mcherror) {
            if (mcherror.data != null) {
                alert(mcherror.data);
                alert('Error in getting records GetAllCoupons');
            }
        });
    }

    function fillLocationListForCoupon() {

        var getData = angularService.getLocationsForCoupons();
        getData.then(function (loc) {
            $scope.locationdata = loc.data;
        }, function (err) {
            if (err.data != null) {
                alert(err.data);
                alert('Error in getting records fillLocationListForCoupon');
            }
        });
    }

    $scope.coupon_file_changed = function (element) {
        $scope.$apply(function (scope) {
            var photofile = element.files[0];
            var reader = new FileReader();
            reader.onload = function (e) {
                // handle onload
                $scope.coupondecdata = btoa(e.target.result);
            };
            reader.readAsDataURL(element.files[0]);
            //reader.readAsText(photofile);
        });
    };

    /* Gift card denomination images */
    $scope.denom1_file_changed = function (element) {
        $scope.$apply(function (scope) {
            var photofile = element.files[0];
            var reader = new FileReader();
            reader.onload = function (e) {
                // handle onload
                $scope.denom1decdata = btoa(e.target.result);
            };
            reader.readAsDataURL(element.files[0]);
            //reader.readAsText(photofile);
        });
    };

    $scope.denom2_file_changed = function (element) {
        $scope.$apply(function (scope) {
            var photofile = element.files[0];
            var reader = new FileReader();
            reader.onload = function (e) {
                // handle onload
                $scope.denom2decdata = btoa(e.target.result);
            };
            reader.readAsDataURL(element.files[0]);
            //reader.readAsText(photofile);
        });
    };

    $scope.denom3_file_changed = function (element) {
        $scope.$apply(function (scope) {
            var photofile = element.files[0];
            var reader = new FileReader();
            reader.onload = function (e) {
                // handle onload
                $scope.denom3decdata = btoa(e.target.result);
            };
            reader.readAsDataURL(element.files[0]);
            //reader.readAsText(photofile);
        });
    };

    $scope.denom4_file_changed = function (element) {
        $scope.$apply(function (scope) {
            var photofile = element.files[0];
            var reader = new FileReader();
            reader.onload = function (e) {
                // handle onload
                $scope.denom4decdata = btoa(e.target.result);
            };
            reader.readAsDataURL(element.files[0]);
            //reader.readAsText(photofile);
        });
    };

    /* Gift card denomination images */

    $scope.merchantlogofile_changed = function (element) {
        $scope.$apply(function (scope) {

            var photofile = element.files[0];
            var reader = new FileReader();
            reader.onload = function (e) {
                // handle onload
                $scope.merchantlogodata = btoa(e.target.result);

            };
            reader.readAsDataURL(element.files[0]);
            //reader.readAsBinaryString(photofile);
        });
    };

    $scope.merchantdecfile_changed = function (element) {
        $scope.$apply(function (scope) {

            var photofile = element.files[0];
            var reader = new FileReader();
            reader.onload = function (e) {
                // handle onload
                $scope.merchantdecdata = btoa(e.target.result);

            };
            reader.readAsDataURL(element.files[0]);
            //reader.readAsBinaryString(photofile);
        });
    };



    function ClearMerchantFields() {
        $scope.MerchantId = "";
        $scope.MerchantName = "";
        $scope.Address1 = "";
        $scope.Address2 = "";
        $scope.locationid = 0;
        $scope.cityid = 0;
        $scope.categoryid = 0;
        $scope.PinCode = "";
        $scope.PhoneNumber = "";
        $scope.merchantlogodata = null;
    }

    function getLicenses() {

        var getData = angularService.getLicenses();

        getData.then(function (lic) {

            $scope.NoOfLicenses = lic.data.NoOfLicenses;
            $scope.AvailableLicenses = lic.data.AvailableLicenses;

            $scope.gridBrands.excessRows = lic.data.brands.length;
            for (i = 0; i < lic.data.brands.length; i++) {
                lic.data.brands[i].subGridOptions = {
                    columnDefs: [
        { name: 'BranchLocation', cellClass: 'htabGrid', headerCellTemplate: '<div></div>', width: "*", resizable: false, cellTemplate: '<div ng-click="grid.appScope.clickMeSub(row)">{{row.entity.BranchLocation}}</div>' },
         { name: 'Delete', cellClass: 'htabGridDelete small-2 columns', headerCellTemplate: '<div></div>', cellTemplate: ' <div style="text-align:center"><i class="fa fa-remove fa-2x red-icon" aria-hidden="true"></i>  </div>', width: "20%", resizable: false }
                    ],
                    enableColumnResize: false,
                    rowHeight: 65,
                    showHeader: false,
                    enablePaginationControls: false,
                    disableRowExpandable: (i % 2 === 0),
                    data: lic.data.brands[i].branches
                }
            }


            if (lic.data.brands != null) {
                if (lic.data.brands.length > 0) {
                    if (lic.data.brands[0].branches != null) {
                        if (lic.data.brands[0].branches.length > 0) {
                            if (lic.data.brands[0].branches[0].staffs != null) {
                                if (lic.data.brands[0].branches[0].staffs.length > 0) {
                                    $scope.gridUsers.excessRows = lic.data.brands[0].branches[0].staffs.length;
                                    $scope.gridUsers.data = lic.data.brands[0].branches[0].staffs;
                                    $scope.gridUsers.totalItems = lic.data.brands[0].branches[0].staffs.length;
                                    if (lic.data.brands[0].branches[0].staffs.length < $scope.numRows) {
                                        $scope.gridUsers.minRowsToShow = lic.data.brands[0].branches[0].staffs.length;
                                    }
                                    else {
                                        $scope.gridUsers.minRowsToShow = $scope.numRows;
                                    }
                                }
                            }
                        }
                    }
                }
            }


            $scope.gridBrands.data = lic.data.brands;
            $scope.gridBrands.totalItems = lic.data.brands.length;
            if (lic.data.brands.length < $scope.numRows) {
                $scope.gridBrands.minRowsToShow = lic.data.brands.length;
            }
            else {
                $scope.gridBrands.minRowsToShow = $scope.numRows;
            }

        }, function (errdata) {
            if (errdata != null) {
                if (errdata.data != null) {
                    alert(errdata.data);
                    alert('Error in getting records getLicenses');
                }
            }
        });
    }

    function editMerchant() {

        var MerchantId = document.getElementsByName('hdnMechantId')[0].value;
        var getData = angularService.getMerchant(parseInt(MerchantId));
        getData.then(function (mch) {

            $scope.merchant = mch.data;
            $scope.MerchantId = mch.data.merchantid;
            $scope.UserId = mch.data.UserId;
            $scope.MerchantName = mch.data.MerchantName;
            $scope.DECName = mch.data.DECName;
            $scope.BuildingName = mch.data.BuildingName;
            $scope.SocietyName = mch.data.SocietyName;
            $scope.Street = mch.data.Street;
            $scope.button1_text = mch.data.button1_text;
            //$scope.button1_url = mch.data.button1_url;

            $scope.button2_text = mch.data.button2_text;
            $scope.button2_url = mch.data.button2_url;

            $scope.button3_text = mch.data.button3_text;
            $scope.button3_url = mch.data.button3_url;

            $scope.button4_text = mch.data.button4_text;
            //$scope.button4_url = mch.data.button4_url;

            //$scope.locationid = mch.data.Location;
            //$scope.cityid = mch.data.City;
            $scope.countryid = mch.data.Country;
            $scope.stateid = mch.data.State;

            $scope.Location = mch.data.Location;
            $scope.City = mch.data.City;
            $scope.Country = mch.data.Country;
            $scope.State = mch.data.State;

            $scope.PinCode = mch.data.PinCode;
            $scope.Email = mch.data.Email;
            $scope.categoryid = mch.data.Category;
            $scope.PhoneNumber = mch.data.PhoneNumber;


            if (mch.data.RunRewardProgram != null) {
                if (mch.data.RunRewardProgram == true) {
                    $scope.rewardSelection = 'yes';
                    //$scope.IsReward = true;
                }
                else {
                    $scope.rewardSelection = 'no';
                    //$scope.IsReward = false;
                }
            }
            else {
                $scope.rewardSelection = 'no';
                //$scope.IsReward = false;
            }


            $scope.JoiningBonus = mch.data.JoiningBonus;
            $scope.usernameSelection = mch.data.PrimaryId;

            //if (mch.data.UserName.indexOf('@') > 0) {
            //    $scope.usernameSelection = 'email';
            //}
            //else {
            //    $scope.usernameSelection = 'phone';
            //}


            if (mch.data.RunRewardProgram != null) {
                if (mch.data.RunRewardProgram == true) {
                    $scope.RedeemSelection = mch.data.RedeemProgram;

                    if ($scope.RedeemSelection == "Options") {
                        if (mch.data.redeemoptions != null) {
                            $scope.condition1 = mch.data.redeemoptions.Option1;
                            $scope.condition2 = mch.data.redeemoptions.Option2;
                            $scope.condition3 = mch.data.redeemoptions.Option3;
                            $scope.condition4 = mch.data.redeemoptions.Option4;
                            $scope.condition5 = mch.data.redeemoptions.Option5;
                        }
                    }

                    if ($scope.RedeemSelection == "Cashback") {
                        if (mch.data.cashbackdetails != null) {
                            if (mch.data.cashbackdetails.IsCashBackPerTransaction == true) {
                                $scope.cashbackSelection = "cashbacktransaction";
                            }
                            else
                                $scope.cashbackSelection = "cashbackpoints";

                            $scope.CashbackRedeemPt = mch.data.cashbackdetails.RedeemPoint;
                            $scope.CashbackRedeemRs = mch.data.cashbackdetails.RedeemRs;
                            $scope.CashbackRs = mch.data.cashbackdetails.FixedCashBack;
                        }
                    }

                    if (mch.data.benefits != null) {
                        $scope.Benefits1 = mch.data.benefits.Benefit1;
                        $scope.Benefits2 = mch.data.benefits.Benefit2;
                        $scope.Benefits3 = mch.data.benefits.Benefit3;
                        $scope.Benefits4 = mch.data.benefits.Benefit4;
                        $scope.Benefits5 = mch.data.benefits.Benefit5;
                    }

                    debugger;
                    if (mch.data.exp != null) {
                        $scope.expid = mch.data.exp.ExpPeriodId;                        
                    }
                }
            }
            $scope.selectedexpiryobject = { Id: $scope.expid };

            if (mch.data.MerchantLogo != null) {
                var CHUNK_SIZE = 0x8000; // arbitrary number here, not too small, not too big
                var index = 0;
                var length = mch.data.MerchantLogo.length;
                var result = '';
                var slice;
                while (index < length) {
                    slice = mch.data.MerchantLogo.slice(index, Math.min(index + CHUNK_SIZE, length)); // `Math.min` is not really necessary here I think
                    result += String.fromCharCode.apply(null, slice);
                    index += CHUNK_SIZE;
                }

                $scope.b64encoded = result;//String.fromCharCode.apply(null, cpn.data.DEC);
                $scope.merchantlogodata = btoa($scope.b64encoded);
            }

            if (mch.data.MerchantDEC != null) {
                var MRCHUNK_SIZE = 0x8000; // arbitrary number here, not too small, not too big
                var MRindex = 0;
                var MRlength = mch.data.MerchantDEC.length;
                var MRresult = '';
                var MRslice;
                while (MRindex < MRlength) {
                    MRslice = mch.data.MerchantDEC.slice(MRindex, Math.min(MRindex + MRCHUNK_SIZE, MRlength)); // `Math.min` is not really necessary here I think
                    MRresult += String.fromCharCode.apply(null, MRslice);
                    MRindex += MRCHUNK_SIZE;
                }


                $scope.b64decencoded = MRresult;//String.fromCharCode.apply(null, cpn.data.DEC);
                $scope.merchantdecdata = btoa($scope.b64decencoded);
            }
            else if (mch.data.merchantDecFromLibrary != null) {
                $scope.b64decencoded = mch.data.merchantDecFromLibrary;
                $scope.merchantDecFromLibrary = mch.data.merchantDecFromLibrary;
                $scope.SelectedImageSrc = mch.data.merchantDecFromLibrary;
            }

            $scope.DECColor = mch.data.DECColor;

            $scope.selectedcatobject = { categoryid: $scope.categoryid };

            $scope.selectedcountryobject = { countryid: $scope.countryid };
            $scope.selectedstateobject = { stateid: $scope.stateid };
            //$scope.selectedcityobject = { cityid: $scope.cityid };
            //$scope.selectedlocobject = { LocationId: $scope.locationid };

            $scope.getStates($scope.selectedcountryobject);
            //$scope.getCities($scope.selectedstateobject);
            //$scope.getLocations($scope.selectedcityobject);

            $scope.RewardRs = mch.data.RewardRs;
            $scope.RewardPts = mch.data.RewardPoints;

            $scope.RedeemPt = mch.data.RedeemPt;
            $scope.RedeemRs = mch.data.RedeemRs;
            $scope.RewardName = mch.data.RewardName;

            //Get reward formula of merchant
            //var getRewardData = angularService.getMerchantRewards(mch.data.UserId);
            //getRewardData.then(function (rwd) {
            //    $scope.RewardName = rwd.data.RewardName;
            //    $scope.RewardRs = rwd.data.RewardRs;
            //    $scope.RewardPts = rwd.data.RewardPoints;
            //});

            ////Get reward formula of merchant
            //var getRedeemData = angularService.getMerchantRedeems(mch.data.UserId);
            //getRedeemData.then(function (red) {
            //    $scope.RedeemPt = red.data.RedeemPt;
            //    $scope.RedeemRs = red.data.RedeemRs;
            //});


            $scope.Action = "Update";

        }, function (errdata) {
            if (errdata != null) {
                if (errdata.data != null) {
                    alert(errdata.data);
                    alert('Error in getting records editMerchant');
                }
            }
        });
    }


    $scope.ClearLogo = function () {

        $scope.b64encoded = null;
        $scope.merchantlogodata = null;
    }

    $scope.AddUpdateMerchant = function (form) {
        var getAction = "Update";
        //if ($scope[form].$valid) {

        if ($scope.RewardName == undefined || $scope.RewardName == "") {
            $scope.RewardName = "My Rewards";
        }
        var merchant = {
            DECName: $scope.MerchantName,
            button1_text: $scope.button1_text,
            //button1_url: $scope.button1_url,
            button2_text: $scope.button2_text,
            button2_url: $scope.button2_url,
            button3_text: $scope.button3_text,
            button3_url: $scope.button3_url,
            button4_text: $scope.button4_text,
            //button4_url: $scope.button4_url,
            BuildingName: $scope.BuildingName,
            SocietyName: $scope.SocietyName,
            Street: $scope.Street,
            Country: $scope.countryid,
            State: $scope.stateid,
            //Location: $scope.locationid,
            //City: $scope.cityid,
            //Country: $scope.Country,
            //State: $scope.State,
            Location: $scope.Location,
            City: $scope.City,
            PinCode: $scope.PinCode,
            Email: $scope.Email,
            Category: $scope.categoryid,
            PhoneNumber: $scope.PhoneNumber,
            MerchantLogo: $scope.merchantlogodata,
            MerchantDEC: $scope.merchantdecdata,
            merchantDecFromLibrary: $scope.SelectedImageSrc,
            RewardName: $scope.RewardName,
            RewardRs: $scope.RewardRs,
            RewardPoints: $scope.RewardPts,
            RedeemRs: $scope.RedeemRs,
            RedeemPt: $scope.RedeemPt,
            DECColor: $scope.DECColor,
            JoiningBonus: $scope.JoiningBonus,
            // RunRewardProgram: $scope.IsReward
        };

        var flag = document.getElementsByName('hdnFlag')[0].value;


        if ($scope.rewardSelection == 'yes') {
            merchant.RunRewardProgram = true;
        }
        else {
            merchant.RunRewardProgram = false;
        }



        if (flag == "showRewardProgram") {
            if ($scope.rewardSelection == 'yes') {
                if ($scope.RewardRs == undefined || $scope.RewardRs == "") {
                    getAction = "";

                    alert("Please enter reward " + $scope.currency);

                }

                if ($scope.RewardPts == undefined || $scope.RewardPts == "") {
                    getAction = "";
                    alert("Please enter reward points");
                }
            }
        }
        //if ($scope.IsReward) {
        //    if ($scope.RedeemSelection == "no") {
        //        merchant.RunRewardProgram = false;
        //    }
        //    else {
        //        merchant.RunRewardProgram = true;
        //    }
        //}
        //else {
        //    merchant.RunRewardProgram = false;
        //}

        merchant.RedeemProgram = $scope.RedeemSelection;

        var redeemoptions =
            {
                Option1: $scope.condition1,
                Option2: $scope.condition2,
                Option3: $scope.condition3,
                Option4: $scope.condition4,
                Option5: $scope.condition5,
            }

        merchant.redeemoptions = redeemoptions;

        var isBenefitsAdded = false;
        if ($scope.Benefits1 != undefined && $scope.Benefits1 != "") {
            isBenefitsAdded = true;
        }
        if ($scope.Benefits2 != undefined && $scope.Benefits2 != "") {
            isBenefitsAdded = true;
        }
        if ($scope.Benefits3 != undefined && $scope.Benefits3 != "") {
            isBenefitsAdded = true;
        }
        if ($scope.Benefits4 != undefined && $scope.Benefits4 != "") {
            isBenefitsAdded = true;
        }
        if ($scope.Benefits5 != undefined && $scope.Benefits5 != "") {
            isBenefitsAdded = true;
        }

        if (isBenefitsAdded) {
            var benefits =
    {
        Benefit1: $scope.Benefits1,
        Benefit2: $scope.Benefits2,
        Benefit3: $scope.Benefits3,
        Benefit4: $scope.Benefits4,
        Benefit5: $scope.Benefits5,
    }

            merchant.benefits = benefits;
        }
        
        var pointscashbackexpiry =
          {
              ExpPeriodId: $scope.expid
          }

        merchant.exp = pointscashbackexpiry;

        var cashbackoptions =
            {
                RedeemPoint: $scope.CashbackRedeemPt,
                RedeemRs: $scope.CashbackRedeemRs,
                FixedCashBack: $scope.CashbackRs
            }

        if ($scope.cashbackSelection == "cashbackpoints") {
            cashbackoptions.IsCashBackPerTransaction = false;
        }
        else if ($scope.cashbackSelection == "cashbacktransaction") {
            cashbackoptions.IsCashBackPerTransaction = true;
        }
        else {
            cashbackoptions.IsCashBackPerTransaction = false;
        }

        merchant.cashbackdetails = cashbackoptions;



        //var getAction = $scope.Action;

        if (getAction == "Update") {
            merchant.MerchantName = $scope.MerchantName;
            merchant.DECName = $scope.DECName;
            merchant.MerchantId = $scope.MerchantId;
            merchant.UserId = $scope.UserId;
            merchant.PhoneNumber = $scope.PhoneNumber;

            var getData = angularService.updateMerchant(merchant);
            getData.then(function (msg) {

                alert(msg.data);
                window.location.href = '/Home/Index';
            }, function () {
                alert('Error in updating merchant');
            });
        }
        //} else {
        //    $scope.showMsgs = true;
        //}
    };

    $scope.AddUpdateReview = function (form) {
        if ($scope[form].$valid) {
            var review = {
                Question1: $scope.Question1,
                Question1Type: "Rating",
                Question2: $scope.Question2,
                Question2Type: "Rating",
                Question3: $scope.Question3,
                Question3Type: "Rating",
                Question4: $scope.Question4,
                Question4Type: "Rating",
                DefaultQuestion: "Share Our DEC with friends.",
                DefaultType: "YesNo"
            };

            var getAction = "Add";
            //var getAction = $scope.Action;


            if (getAction == "Update") {
                review.reviewid = $scope.reviewid;

                var getData = angularService.updateReview(review);
                getData.then(function (msg) {
                    alert(msg.data);

                }, function () {
                    alert('Error in updating review');
                });
            } else {
                var getData = angularService.AddReview(review);
                getData.then(function (msg) {
                    alert(msg.data);

                }, function () {
                    alert('Error in adding review');
                });
            }
        } else {
            $scope.showMsgs = true;
        }
    };

    //**************************************************//
    //Set Events Coupons
    $scope.BCouponChanged = function (cpn) {
        if (cpn != null) {
            $scope.bcouponid = cpn.couponid;
        }
    }

    $scope.ACouponChanged = function (cpn) {
        if (cpn != null) {
            $scope.acouponid = cpn.couponid;
        }
    }

    $scope.RCouponChanged = function (cpn) {
        if (cpn != null) {
            $scope.rcouponid = cpn.couponid;
        }
    }

    $scope.SCouponChanged = function (cpn) {
        if (cpn != null) {
            $scope.scouponid = cpn.couponid;
        }
    }

    $scope.SetEventCoupons = function (form) {
        if ($scope[form].$valid) {
            var eventcoupons = {
                birthdaycoupon: $scope.bcouponid,
                anncoupon: $scope.acouponid,
                reviewcoupon: $scope.rcouponid,
                sharecoupon: $scope.scouponid
            };

            var getData = angularService.SetCoupons(eventcoupons);
            getData.then(function (msg) {
                alert(msg.data);
                window.location.href = '/Home/Index';
            }, function () {
                alert('Error in setting event coupons');
            });
        } else {
            $scope.showMsgs = true;
        }
    };


    $scope.AddLocation = function () {

        if ($scope.AvailableLicenses > 0) {
            fillBrandList();
            $scope.BrandId = 0;
            $scope.BrandManagerId = 0;
            $scope.BranchId = 0;
            $scope.BranchManagerId = 0;
            $scope.b64encoded = null;
            $scope.divBrands = false;
            $scope.divAddBrand = false;
            $scope.divAddLocation = true;
            $scope.divAddStaff = false;
            $scope.AddUpdateLocationHeading = angularService.getString("CreateLocation", $rootScope.languagecode);
        }
        else {
            alert(angularService.getString("NoLicensesPrompt", $rootScope.languagecode));
        }
    }

    $scope.AddStaff = function () {
        $scope.BrandId = 0;
        $scope.BranchId = 0;
        $scope.StaffId = 0;
        $scope.StaffMasterId = 0;
        fillBrandList();
        $scope.b64encoded = null;
        $scope.divBrands = false;
        $scope.divAddBrand = false;
        $scope.divAddLocation = false;
        $scope.divAddStaff = true;
        $scope.AddUpdateStaffHeading = angularService.getString("CreateStaff", $rootScope.languagecode);

    }

    $scope.AddBrand = function () {
        if ($scope.gridBrands.data.length == $scope.NoOfLicenses) {
            alert(angularService.getString("NoLicensesPrompt", $rootScope.languagecode));
        }
        else if ($scope.AvailableLicenses > 0) {
            $scope.BrandId = 0;
            $scope.BrandManagerId = 0;
            $scope.divBrands = false;
            $scope.divAddBrand = true;
            $scope.divAddLocation = false;
            $scope.divAddStaff = false;
            $scope.AddUpdateHeading = angularService.getString("CreateBrand", $rootScope.languagecode);
        }
        else {
            alert(angularService.getString("NoLicensesPrompt", $rootScope.languagecode));
        }
    }

    $scope.AddNewStaff = function () {
        if ($scope.BrandId == undefined) {
            var text = angularService.getString("SelectBrand", $rootScope.languagecode);
            alert(text);
        }
        else if ($scope.BranchId == undefined) {
            var text = angularService.getString("SelectBranch", $rootScope.languagecode);
            alert(text);
        }
        else {
            var ccode = $scope.selectedcountrycodeobject;
            var staff = {
                BrandId: $scope.BrandId,
                BranchId: $scope.BranchId,
                StaffName: $scope.StaffName,
                PrimaryId: $scope.PrimaryStaffIDSelection
            };

            if ($scope.AllowSendCoupon == "yes") {
                staff.IsCouponSendAllowed = true;
            }
            else
                staff.IsCouponSendAllowed = false;

            if ($scope.StaffId == 0) {
                var merchant = {
                    MerchantName: $scope.StaffName,
                    MerchantLogo: $scope.merchantlogodata,
                    Email: $scope.Email,
                    PhoneNumber: ccode.CountryCode + " " + $scope.PhoneNumber,
                    staff: staff
                };

                var getData = angularService.AddStaff(merchant);
                getData.then(function (msg) {
                    alert(msg.data);
                    getLicenses();
                    $scope.divBrands = true;
                    $scope.divAddBrand = false;
                    $scope.divAddLocation = false;
                    $scope.divAddStaff = false;
                }, function () {
                    alert('Error in adding staff');
                });
            }
            else {
                staff.StaffId = $scope.StaffId;

                var getData = angularService.UpdateStaff(staff);
                getData.then(function (msg) {
                    alert(msg.data);
                    getLicenses();
                    $scope.divBrands = true;
                    $scope.divAddBrand = false;
                    $scope.divAddLocation = false;
                    $scope.divAddStaff = false;
                }, function () {
                    alert('Error in updating location');
                });
            }
        }

    }

    $scope.AddNewLocation = function () {

        if ($scope.BrandId != undefined) {

            var branch = {
                BrandId: $scope.BrandId,
                BranchManagerName: $scope.LocationManager,
                BranchLocation: $scope.BranchLocation,
                PrimaryId: $scope.PrimaryLocIDSelection
            };

            var ccode = $scope.selectedcountrycodeobject;

            if ($scope.AllowMenuCreation == "yes") {
                branch.IsMenuAllowed = true;
            }
            else
                branch.IsMenuAllowed = false;

            if ($scope.AllowCouponCreation == "yes") {
                branch.IsCouponAllowed = true;
            }
            else
                branch.IsCouponAllowed = false;

            if ($scope.AllowEventCouponCreation == "yes") {
                branch.IsEventCouponsAllowed = true;
            }
            else
                branch.IsEventCouponsAllowed = false;

            if ($scope.AllowUserCreation == "yes") {
                branch.IsAddUserAllowed = true;
            }
            else
                branch.IsAddUserAllowed = false;

            if ($scope.BranchId == 0) {
                var merchant = {
                    MerchantName: $scope.LocationManager,
                    MerchantLogo: $scope.merchantlogodata,
                    Email: $scope.Email,
                    PhoneNumber: ccode.CountryCode + " " + $scope.PhoneNumber,
                    branch: branch
                };

                var getData = angularService.AddBranch(merchant);
                getData.then(function (msg) {
                    alert(msg.data);
                    getLicenses();
                    $scope.divBrands = true;
                    $scope.divAddBrand = false;
                    $scope.divAddLocation = false;
                    $scope.divAddStaff = false;
                }, function () {
                    alert('Error in adding location');
                });
            }
            else {
                branch.BranchId = $scope.BranchId;

                var getData = angularService.UpdateBranch(branch);
                getData.then(function (msg) {
                    alert(msg.data);
                    getLicenses();
                    $scope.divBrands = true;
                    $scope.divAddBrand = false;
                    $scope.divAddLocation = false;
                    $scope.divAddStaff = false;
                }, function () {
                    alert('Error in updating location');
                });
            }
        }
        else {
            var text = angularService.getString("SelectBrand", $rootScope.languagecode);
            alert(text);
        }
    }



    $scope.AddNewBrand = function () {

        var brand = {
            BrandName: $scope.BrandName,
            BrandManagerName: $scope.BrandManager,
            Category: $scope.categoryid,
            NoOfLocations: $scope.NoOfLocations,
            PrimaryId: $scope.PrimaryIDSelection
        };

        var ccode = $scope.selectedcountrycodeobject;

        if ($scope.BrandId == 0) {
            var merchant = {
                MerchantName: $scope.BrandManager,
                MerchantLogo: $scope.merchantlogodata,
                Email: $scope.Email,
                PhoneNumber: ccode.CountryCode + " " + $scope.PhoneNumber,
                brand: brand
            };


            var getData = angularService.AddBrand(merchant);
            getData.then(function (msg) {

                alert(msg.data);
                getLicenses();
                $scope.divBrands = true;
                $scope.divAddBrand = false;
                $scope.divAddLocation = false;
                $scope.divAddStaff = false;
            }, function () {
                alert('Error in adding branch');
            });
        }
        else {
            brand.BrandId = $scope.BrandId;

            var getData = angularService.UpdateBrand(brand);
            getData.then(function (msg) {
                alert(msg.data);
                getLicenses();
                $scope.divBrands = true;
                $scope.divAddBrand = false;
                $scope.divAddLocation = false;
                $scope.divAddStaff = false;
            }, function () {
                alert('Error in updating brand');
            });
        }
    }
    //**************************************************//
    $scope.AddUpdateCoupon = function (form) {

        if ($scope[form].$valid) {

            //if ($scope.Discount == undefined && $scope.PercentageOff == undefined) {
            //    alert('Please enter either percentage off or discount');
            //}
            //else if ($scope.Discount == "" && $scope.PercentageOff == "") {
            //    alert('Please enter either percentage off or discount');
            //}
            var date = new Date();
            var fromdate = $scope.ValidFrom;
            var todate = $scope.ValidTill;

            $scope.today = date.getFullYear() + '-' + ('0' + (date.getMonth() + 1)).slice(-2) + '-' + ('0' + date.getDate()).slice(-2);
            if (fromdate != undefined) {
                fromdate = fromdate.getFullYear() + '-' + ('0' + (fromdate.getMonth() + 1)).slice(-2) + '-' + ('0' + fromdate.getDate()).slice(-2);
            }

            if (todate != undefined) {
                todate = todate.getFullYear() + '-' + ('0' + (todate.getMonth() + 1)).slice(-2) + '-' + ('0' + todate.getDate()).slice(-2);
            }

            if ((fromdate != undefined) && (fromdate < $scope.today)) {
                var text = angularService.getString("DateTimePrompt", $rootScope.languagecode);
                alert(text);
            }
            else if ((fromdate != undefined && todate != undefined) && (fromdate > todate)) {
                var text = angularService.getString("DateTimePrompt", $rootScope.languagecode);
                alert(text);
            }
                //else if ($scope.ValidAt == undefined) {
                //    alert('Please enter Valid At Location');
                //}
            else {
                var coupon = {
                    CouponTitle: $scope.CouponTitle,
                    CouponDetails: $scope.CouponDetails,
                    ValidFrom: $scope.ValidFrom,
                    ValidTill: $scope.ValidTill,
                    PercentageOff: $scope.PercentageOff,
                    Discount: $scope.Discount,
                    AboveAmount: $scope.AboveAmount,
                    DEC: $scope.coupondecdata,
                    //locations: JSON.stringify($scope.locationmodel),
                    //ValidAtLocation: JSON.stringify($scope.locationmodel),
                    ValidAtLocation: $scope.ValidAt,
                    MaxDiscount: $scope.MaxDiscount,
                    QRCode: $scope.couponqrcodedata,
                    // ShareWithAll: $scope.shareondec,
                    MaxCoupons: $scope.MaxCoupons
                };

                var c1, c2, c3, c4, c5;
                if ($scope.couponcondition1 != undefined)
                    c1 = $scope.couponcondition1
                else
                    c1 = "";

                if ($scope.couponcondition2 != undefined)
                    c2 = $scope.couponcondition2
                else
                    c2 = "";

                if ($scope.couponcondition3 != undefined)
                    c3 = $scope.couponcondition3
                else
                    c3 = "";

                if ($scope.couponcondition4 != undefined)
                    c4 = $scope.couponcondition4
                else
                    c4 = "";

                if ($scope.couponcondition5 != undefined)
                    c5 = $scope.couponcondition5
                else
                    c5 = "";

                $scope.conditions = [
     {
         "Condition": c1
     },
     {
         "Condition": c2
     },
     {
         "Condition": c3
     },
     {
         "Condition": c4
     },
     {
         "Condition": c5
     }
                ];


                coupon.conditions = $scope.conditions



                if ($scope.checkboxSelection == "percentage") {
                    coupon.Discount = null;
                }

                if ($scope.checkboxSelection == "discount") {
                    coupon.PercentageOff = null;
                }

                var getAction = $scope.Action;

                if (getAction == "Update") {

                    coupon.couponid = $scope.couponid;
                    coupon.CouponCode = $scope.CouponCode;
                    coupon.MerchantId = $scope.MerchantId;
                    coupon.categoryid = $scope.categoryid;

                    var getData = angularService.updateCoupon(coupon);

                    getData.then(function (msg) {
                        alert(msg.data);
                        window.location.href = '/Merchant/CouponList';
                    }, function () {
                        alert('Error in updating coupon');
                    });
                } else {
                    var getData = angularService.AddCoupon(coupon);
                    getData.then(function (msg) {

                        if (msg.data != "-1") {
                            var text = angularService.getString("CouponAddedPrompt", $rootScope.languagecode);
                            alert(text);
                            window.location.href = '/Merchant/SendCoupon?CouponId=' + msg.data;
                        }
                        else {
                            alert("Invalid Merchant Details");
                        }

                    }, function () {
                        alert('Error in adding coupon');
                    });
                }
            }
        } else {
            $scope.showMsgs = true;
        }
    };

    function ClearCouponFields() {
        $scope.couponid = "";
        $scope.CouponTitle = "";
        $scope.CouponDetails = "";
        $scope.ValidFrom = "";
        $scope.locationid = 0;
        $scope.cityid = 0;
        $scope.categoryid = 0;
        $scope.ValidTill = "";
        $scope.PercentageOff = "";
        $scope.Discount = "";
        $scope.AboveAmount = "";
        $scope.coupondecdata = null;
    }

    $scope.editStaff = function (staff) {

        var getData = angularService.getStaff(staff.StaffId);

        getData.then(function (stf) {

            $scope.AddUpdateStaffHeading = angularService.getString("UpdateStaff", $rootScope.languagecode);

            $scope.BranchId = stf.data.BranchId
            $scope.BrandId = stf.data.BrandId;
            $scope.StaffId = stf.data.StaffId;

            $scope.StaffName = stf.data.StaffName;

            $scope.PrimaryId = stf.data.PrimaryId;
            $scope.PrimaryStaffIDSelection = stf.data.PrimaryId;

            if (stf.data.IsCouponSendAllowed == true) {
                $scope.AllowSendCoupon = "yes"
            }
            else
                $scope.AllowSendCoupon = "no";

            $scope.MerchantId = stf.data.MerchantId;
            $scope.StaffMasterId = stf.data.StaffMasterId;

            var getStaffManager = angularService.getMerchant(stf.data.StaffMasterId);

            getStaffManager.then(function (mgr) {

                $scope.PhoneNumber = mgr.data.PhoneNumber;
                var array = $scope.PhoneNumber.split(' ');
                if (array.length == 2) {
                    $scope.PhoneNumber = array[1];
                    var getCountryData = angularService.getCountryFromCountryCode(array[0]);
                    getCountryData.then(function (mch) {

                        $scope.selectedcountrycodeobject = { countryid: mch.data.countryid, CountryCode: mch.data.CountryCode };
                    }, function (err) {
                        if (err != null) {
                            if (err.data != null) {
                                alert(err.data);
                                alert('Error in getting records editBrand/getcountryfromcode');
                            }
                        }

                    });
                }


                $scope.Email = mgr.data.Email;

                if (mgr.data.MerchantLogo != null) {
                    var CHUNK_SIZE = 0x8000; // arbitrary number here, not too small, not too big
                    var index = 0;
                    var length = mgr.data.MerchantLogo.length;
                    var result = '';
                    var slice;
                    while (index < length) {
                        slice = mgr.data.MerchantLogo.slice(index, Math.min(index + CHUNK_SIZE, length)); // `Math.min` is not really necessary here I think
                        result += String.fromCharCode.apply(null, slice);
                        index += CHUNK_SIZE;
                    }

                    $scope.b64encoded = result;//String.fromCharCode.apply(null, cpn.data.DEC);
                    $scope.merchantlogodata = btoa($scope.b64encoded);
                }

            });

            var getBranches = angularService.getBranches(stf.data.BrandId);

            getBranches.then(function (brch) {

                $scope.BranchList = brch.data;
            });


            $scope.Action = "Update";
            $scope.divAddBrand = false;
            $scope.divBrands = false;
            $scope.divAddLocation = false;
            $scope.divAddStaff = true;
            $scope.selectedbrandobject = { BrandId: $scope.BrandId };

            $scope.selectedbranchobject = { BranchId: $scope.BranchId };

        }, function (cpnerr) {
            // alert(cpnerr.data);
            if (cpnerr != null) {
                if (cpnerr.data != null) {
                    alert(cpnerr.data);
                    alert('Error in getting records editStaff');
                }
            }
        });
    }


    function editLocation(branch) {

        var getData = angularService.getBranch(branch.BranchId);

        getData.then(function (bch) {

            $scope.AddUpdateLocationHeading = angularService.getString("UpdateLocation", $rootScope.languagecode);

            $scope.gridUsers.excessRows = bch.data.staffs.length;
            $scope.gridUsers.data = bch.data.staffs;
            $scope.gridUsers.totalItems = bch.data.staffs.length;
            if (bch.data.staffs.length < $scope.numRows) {
                $scope.gridUsers.minRowsToShow = bch.data.staffs.length;
            }
            else {
                $scope.gridUsers.minRowsToShow = $scope.numRows;
            }

            $scope.BranchId = bch.data.BranchId
            $scope.BrandId = bch.data.BrandId;
            $scope.BranchLocation = bch.data.BranchLocation;

            $scope.PrimaryId = bch.data.PrimaryId;
            $scope.PrimaryLocIDSelection = bch.data.PrimaryId;

            if (bch.data.IsMenuAllowed == true) {
                $scope.AllowMenuCreation = "yes"
            }
            else
                $scope.AllowMenuCreation = "no";

            if (bch.data.IsCouponAllowed == true) {
                $scope.AllowCouponCreation = "yes"
            }
            else
                $scope.AllowCouponCreation = "no";

            if (bch.data.IsEventCouponsAllowed == true) {
                $scope.AllowEventCouponCreation = "yes"
            }
            else
                $scope.AllowEventCouponCreation = "no";

            if (bch.data.IsAddUserAllowed == true) {
                $scope.AllowUserCreation = "yes"
            }
            else
                $scope.AllowUserCreation = "no";

            $scope.LocationManager = bch.data.BranchManagerName;

            $scope.MerchantId = bch.data.MerchantId;
            $scope.BranchManagerId = bch.data.BranchManagerId;

            var getBranchManager = angularService.getMerchant(bch.data.BranchManagerId);

            getBranchManager.then(function (mgr) {
                $scope.PhoneNumber = mgr.data.PhoneNumber;

                var array = $scope.PhoneNumber.split(' ');
                if (array.length == 2) {
                    $scope.PhoneNumber = array[1];
                    var getCountryData = angularService.getCountryFromCountryCode(array[0]);
                    getCountryData.then(function (mch) {

                        $scope.selectedcountrycodeobject = { countryid: mch.data.countryid, CountryCode: mch.data.CountryCode };
                    }, function (err) {
                        if (err != null) {
                            if (err.data != null) {
                                alert(err.data);
                                alert('Error in getting records editBrand/getcountryfromcode');
                            }
                        }

                    });
                }


                $scope.Email = mgr.data.Email;

                if (mgr.data.MerchantLogo != null) {
                    var CHUNK_SIZE = 0x8000; // arbitrary number here, not too small, not too big
                    var index = 0;
                    var length = mgr.data.MerchantLogo.length;
                    var result = '';
                    var slice;
                    while (index < length) {
                        slice = mgr.data.MerchantLogo.slice(index, Math.min(index + CHUNK_SIZE, length)); // `Math.min` is not really necessary here I think
                        result += String.fromCharCode.apply(null, slice);
                        index += CHUNK_SIZE;
                    }

                    $scope.b64encoded = result;//String.fromCharCode.apply(null, cpn.data.DEC);
                    $scope.merchantlogodata = btoa($scope.b64encoded);
                }

            });


            $scope.Action = "Update";
            $scope.divAddBrand = false;
            $scope.divBrands = false;
            $scope.divAddLocation = true;
            $scope.divAddStaff = false;
            $scope.selectedbrandobject = { BrandId: $scope.BrandId };

        }, function (cpnerr) {
            // alert(cpnerr.data);
            if (cpnerr != null) {
                if (cpnerr.data != null) {
                    alert(cpnerr.data);
                    alert('Error in getting records editLocation');
                }
            }
        });
    }

    $scope.editBrand = function (brand) {

        var getData = angularService.getBrand(brand.BrandId);

        getData.then(function (bnd) {

            $scope.AddUpdateHeading = angularService.getString("UpdateBrand", $rootScope.languagecode);
            $scope.BrandId = bnd.data.BrandId;
            $scope.BrandName = bnd.data.BrandName;
            $scope.Category = bnd.data.Category;
            $scope.NoOfLocations = bnd.data.NoOfLocations;
            $scope.PrimaryId = bnd.data.PrimaryId;
            $scope.PrimaryIDSelection = bnd.data.PrimaryId;
            $scope.MerchantId = bnd.data.MerchantId;
            $scope.BrandManagerId = bnd.data.BrandManagerId;
            $scope.BrandManager = bnd.data.BrandManagerName;

            var getBrandManager = angularService.getMerchant(bnd.data.BrandManagerId);

            getBrandManager.then(function (mgr) {

                $scope.PhoneNumber = mgr.data.PhoneNumber;
                var array = $scope.PhoneNumber.split(' ');
                if (array.length == 2) {
                    $scope.PhoneNumber = array[1];
                    var getCountryData = angularService.getCountryFromCountryCode(array[0]);
                    getCountryData.then(function (mch) {

                        $scope.selectedcountrycodeobject = { countryid: mch.data.countryid, CountryCode: mch.data.CountryCode };
                    }, function (err) {
                        if (err != null) {
                            if (err.data != null) {
                                alert(err.data);
                                alert('Error in getting records editBrand/getcountryfromcode');
                            }
                        }

                    });
                }

                $scope.Email = mgr.data.Email;

                if (mgr.data.MerchantLogo != null) {
                    var CHUNK_SIZE = 0x8000; // arbitrary number here, not too small, not too big
                    var index = 0;
                    var length = mgr.data.MerchantLogo.length;
                    var result = '';
                    var slice;
                    while (index < length) {
                        slice = mgr.data.MerchantLogo.slice(index, Math.min(index + CHUNK_SIZE, length)); // `Math.min` is not really necessary here I think
                        result += String.fromCharCode.apply(null, slice);
                        index += CHUNK_SIZE;
                    }

                    $scope.b64encoded = result;//String.fromCharCode.apply(null, cpn.data.DEC);
                    $scope.merchantlogodata = btoa($scope.b64encoded);
                }

            });


            $scope.Action = "Update";
            $scope.divAddBrand = true;
            $scope.divBrands = false;
            $scope.divAddLocation = false;
            $scope.divAddStaff = false;
            $scope.selectedcatobject = { categoryid: $scope.Category };

        }, function (cpnerr) {
            // alert(cpnerr.data);
            if (cpnerr != null) {
                if (cpnerr.data != null) {
                    alert(cpnerr.data);
                    alert('Error in getting records editBrand');
                }
            }
        });
    }



    $scope.editCoupon = function (coupon) {

        var getData = angularService.getCoupon(coupon.couponid);

        getData.then(function (cpn) {

            $scope.coupon = cpn.data;
            $scope.couponid = cpn.data.couponid;
            $scope.CouponTitle = cpn.data.CouponTitle;
            $scope.CouponCode = cpn.data.CouponCode;
            $scope.CouponDetails = cpn.data.CouponDetails;
            $scope.MerchantId = cpn.data.MerchantId;
            if (cpn.data.ValidFrom != null) {
                $scope.ValidFrom = cpn.data.ValidFrom;

                $scope.ValidFrom = $scope.ValidFrom.replace('/Date(', '');

                $scope.ValidFrom = $scope.ValidFrom.replace(')/', '');
                $scope.ValidFrom = new Date(parseInt($scope.ValidFrom));
            }


            if (cpn.data.ValidTill != null) {
                $scope.ValidTill = cpn.data.ValidTill;

                $scope.ValidTill = $scope.ValidTill.replace('/Date(', '');

                $scope.ValidTill = $scope.ValidTill.replace(')/', '');
                $scope.ValidTill = new Date(parseInt($scope.ValidTill));
            }

            $scope.categoryid = cpn.data.categoryid;

            if (cpn.data.PercentageOff != null)
                $scope.checkboxSelection = "percentage";
            else
                $scope.checkboxSelection = "discount";

            $scope.PercentageOff = cpn.data.PercentageOff;
            $scope.Discount = cpn.data.Discount;
            $scope.MaxDiscount = cpn.data.MaxDiscount;
            $scope.AboveAmount = cpn.data.AboveAmount;
            //$scope.ValidAtCity = cpn.data.ValidAtCity;
            //$scope.cityid = cpn.data.ValidAtCity;
            // $scope.locationid = cpn.data.ValidAtLocation;
            $scope.ValidAtLocation = cpn.data.ValidAtLocation;

            $scope.MaxCoupons = cpn.data.MaxCoupons;

            //if (cpn.data.ShareWithAll != null) {
            //    if (cpn.data.ShareWithAll == 1) {
            //        $scope.shareondec = true;
            //    }
            //    else {
            //        $scope.shareondec = false;
            //    }
            //}
            //else
            //    $scope.shareondec = false;

            //$scope.selectedcityobject = { cityid: $scope.cityid };

            //$scope.selectedlocobject = { id: parseInt($scope.locationid) };
            //$scope.locationmodel.push($scope.selectedlocobject);

            //$scope.locationmodel = json.stringify($scope.selectedlocobject);

            //$scope.getLocations($scope.selectedcityobject);

            if (cpn.data.DEC != null) {
                var CHUNK_SIZE = 0x8000; // arbitrary number here, not too small, not too big
                var index = 0;
                var length = cpn.data.DEC.length;
                var result = '';
                var slice;
                while (index < length) {
                    slice = cpn.data.DEC.slice(index, Math.min(index + CHUNK_SIZE, length)); // `Math.min` is not really necessary here I think
                    result += String.fromCharCode.apply(null, slice);
                    index += CHUNK_SIZE;
                }


                $scope.b64encoded = result;//String.fromCharCode.apply(null, cpn.data.DEC);
                $scope.coupondecdata = btoa($scope.b64encoded);
            }
            //uncomment to generate qr code
            //if (cpn.data.QRCode != null) {
            //    var QRCHUNK_SIZE = 0x8000; // arbitrary number here, not too small, not too big
            //    var QRindex = 0;
            //    var QRlength = cpn.data.QRCode.length;
            //    var QRresult = '';
            //    var QRslice;
            //    while (QRindex < QRlength) {
            //        QRslice = cpn.data.QRCode.slice(QRindex, Math.min(QRindex + QRCHUNK_SIZE, QRlength)); // `Math.min` is not really necessary here I think
            //        QRresult += String.fromCharCode.apply(null, QRslice);
            //        QRindex += QRCHUNK_SIZE;
            //    }


            //    $scope.b64encodedqr = btoa(QRresult);// String.fromCharCode.apply(null, cpn.data.QRCode);
            //    $scope.couponqrcodedata = $scope.b64encodedqr;
            //}

            if (cpn.data.conditions != null) {
                for (var i = 0; i < cpn.data.conditions.length; i++) {
                    if (cpn.data.conditions[i].Condition != "") {
                        if (i == 0)
                            $scope.couponcondition1 = cpn.data.conditions[i].Condition;
                        if (i == 1)
                            $scope.couponcondition2 = cpn.data.conditions[i].Condition;
                        if (i == 2)
                            $scope.couponcondition3 = cpn.data.conditions[i].Condition;
                        if (i == 3)
                            $scope.couponcondition4 = cpn.data.conditions[i].Condition;
                        if (i == 4)
                            $scope.couponcondition5 = cpn.data.conditions[i].Condition;
                    }
                }
            }
            $scope.Action = "Update";
            $scope.divCoupon = true;
            $scope.divCoupons = false;
        }, function (cpnerr) {
            // alert(cpnerr.data);
            if (cpnerr.data != null) {
                alert(cpnerr.data);
                alert('Error in getting records editCoupon');
            }
        });
    }

    $scope.NavigateBack = function () {

        //Go back to home page        
        if ($rootScope.divShowMessage == true) {
            $rootScope.divRedeemCoupon = false;
            $rootScope.divConsumerProfile = true;
            $rootScope.divShowMessage = false;
        }
        else if ($rootScope.divConsumerProfile == true) {
            $rootScope.divRedeemCoupon = true;
            $rootScope.divConsumerProfile = false;
            $rootScope.divShowMessage = false;
        }
        else {
            window.history.back();
        }
    }


    $scope.Redeem = function () {

        if ($scope.CouponCode == undefined) {
            var text = angularService.getString("CouponCodePrompt", $rootScope.languagecode);
            alert(text);
        }
        else {
            var getData = angularService.RedeemCoupon($scope.CouponCode);
            getData.then(function (msg) {

                if (msg.data == "Valid Coupon") {
                    $rootScope.divRedeemCoupon = false;
                    $rootScope.divShowMessage = false;
                }
                else {
                    alert(msg.data);
                }
            }, function () {
                alert('Error in updating coupon');
            });
        }
    };

    $scope.GetConsumerCouponPoints = function () {
        $rootScope.searchButtonText = true;

        $rootScope.divRedeemCoupon = false;
        $rootScope.divShowMessage = false;
        $rootScope.divConsumerProfile = true;


        if ($scope.CustPhoneNumber == undefined && $scope.Email == undefined) {
            var text = angularService.getString("MobileNoEmailPrompt", $rootScope.languagecode);
            alert(text);
        }
        else if ($scope.CustPhoneNumber == "" && $scope.Email == "") {
            var text = angularService.getString("MobileNoEmailPrompt", $rootScope.languagecode);
            alert(text);
        }
        else if ($scope.CustPhoneNumber == "" && $scope.Email == undefined) {
            var text = angularService.getString("MobileNoEmailPrompt", $rootScope.languagecode);
            alert(text);
        }
        else if ($scope.CustPhoneNumber == undefined && $scope.Email == "") {
            var text = angularService.getString("MobileNoEmailPrompt", $rootScope.languagecode);
            alert(text);
        }
        else {
            var MobileNo = "";
            var ccode = $scope.selectedcountrycodeobject;

            if ($scope.CustPhoneNumber != undefined) {
                if ($scope.CustPhoneNumber != "") {
                    MobileNo = ccode.CountryCode + " " + $scope.CustPhoneNumber;
                }
            }

            if (MobileNo == "") {
                if ($scope.Email != undefined) {
                    if ($scope.Email != "") {
                        MobileNo = $scope.Email;
                    }
                }
            }

            var getConsumerData = angularService.getNoOfVisits(MobileNo);

            getConsumerData.then(function (mch) {
                $scope.NoOfVisits = mch.data.NoOfVisits;
                $scope.NoOfPoints = mch.data.NoOfPoints;
                $scope.IsCashback = mch.data.iscashback;

                $scope.ConsumerName = "";

                if (mch.data.ConsumerName != null)
                    $scope.ConsumerName = mch.data.ConsumerName;

                $scope.DOA = null;
                if (mch.data.DOA != null) {
                    $scope.DOA = mch.data.DOA;
                    $scope.DOA = $scope.DOA.replace('/Date(', '');
                    $scope.DOA = $scope.DOA.replace(')/', '');
                    $scope.DOA = new Date(parseInt($scope.DOA)).toDateString();
                }

                $scope.DOB = null;
                if (mch.data.DOB != null) {
                    $scope.DOB = mch.data.DOB;
                    $scope.DOB = $scope.DOB.replace('/Date(', '');
                    $scope.DOB = $scope.DOB.replace(')/', '');
                    $scope.DOB = new Date(parseInt($scope.DOB)).toDateString();
                }

                if (mch.data.NoOfPoints == 0) {
                    $rootScope.divRedeemPoints = false;
                }
                else {
                    $rootScope.divRedeemPoints = true;
                }

                $scope.Coupons = mch.data.CouponList;
                if (mch.data.CouponList.length == 0) {
                    $rootScope.divCoupons = false;
                }
                else {
                    $scope.gridCustOptions.excessRows = mch.data.CouponList.length;
                    $scope.gridCustOptions.data = mch.data.CouponList;
                    $scope.gridCustOptions.totalItems = mch.data.CouponList.length;
                    if (mch.data.CouponList.length < $scope.numRows) {
                        $scope.gridCustOptions.minRowsToShow = mch.data.CouponList.length;
                    }
                    else {
                        $scope.gridCustOptions.minRowsToShow = $scope.numRows;
                    }

                    $rootScope.divCoupons = true;
                }

                $scope.GiftCards = mch.data.GiftCards;
                if (mch.data.GiftCards.length == 0) {
                    $rootScope.divGiftCards = false;
                }
                else {
                    $scope.gridGiftOptions.excessRows = mch.data.GiftCards.length;
                    $scope.gridGiftOptions.data = mch.data.GiftCards;
                    $scope.gridGiftOptions.totalItems = mch.data.GiftCards.length;
                    if (mch.data.GiftCards.length < $scope.numRows) {
                        $scope.gridGiftOptions.minRowsToShow = mch.data.GiftCards.length;
                    }
                    else {
                        $scope.gridGiftOptions.minRowsToShow = $scope.numRows;
                    }

                    $rootScope.divGiftCards = true;
                }

                $rootScope.searchButtonText = false;
            }, function (err) {
                $rootScope.searchButtonText = false;
                if (err != null) {
                    if (err.data != null) {
                        alert(err.data);
                        alert('Error in getting records getNoOfVisits');
                    }
                }
            });
        }
    };

    $scope.GetCouponFromCode = function () {

        if ($scope.CouponCode == undefined) {
            var text = angularService.getString("CouponCodePrompt", $rootScope.languagecode);
            alert(text);
        }
        else if ($("#hdnCouponCode").val() == undefined) {
            var text = angularService.getString("CouponCodePrompt", $rootScope.languagecode);
            alert(text);
        }
        else if ($scope.CustPhoneNumber == undefined) {
            var text = angularService.getString("MobileNoPrompt", $rootScope.languagecode);
            alert(text);
        }
        else if ($scope.BillAmount == undefined) {
            var text = angularService.getString("BillAmountPrompt", $rootScope.languagecode);
            alert(text);
        }
        else {
            if ($scope.CouponCode == undefined)
                $scope.couponcode = $("#hdnCouponCode").val();
            var getData = angularService.CheckCouonValidity($scope.CouponCode, $scope.BillAmount, $scope.CustPhoneNumber);
            getData.then(function (msg) {
                if (String(msg.data).indexOf("Coupon is Valid.") >= 0) {
                    $scope.divRedeemCoupon = false;
                    $scope.validitymsg = msg.data;
                    $scope.divShowMessage = true;
                }
                else {
                    $scope.divRedeemCoupon = false;
                    $scope.validitymsg = "";
                    $scope.divShowMessage = false;
                    alert(msg.data);
                }
            }, function () {
                alert('Error in updating coupon');
            });
        }
    };

    $scope.VerifyPoints = function () {

        if ($scope.NoOfPoints == 0) {
            var text = angularService.getString("NoPointsPrompt", $rootScope.languagecode);
            alert(text);
        }
        if ($scope.CustPhoneNumber == undefined && $scope.Email == undefined) {
            var text = angularService.getString("MobileNoEmailPrompt", $rootScope.languagecode);
            alert(text);
        }
        else if ($scope.CustPhoneNumber == "" && $scope.Email == "") {
            var text = angularService.getString("MobileNoEmailPrompt", $rootScope.languagecode);
            alert(text);
        }
        else if ($scope.CustPhoneNumber == "" && $scope.Email == undefined) {
            var text = angularService.getString("MobileNoEmailPrompt", $rootScope.languagecode);
            alert(text);
        }
        else if ($scope.CustPhoneNumber == undefined && $scope.Email == "") {
            var text = angularService.getString("MobileNoEmailPrompt", $rootScope.languagecode);
            alert(text);
        }
        else if ($scope.RedeemPoints == undefined) {
            var text = angularService.getString("EnterPointsPrompt", $rootScope.languagecode);
            alert(text);
        }
        else if ($scope.NoOfPoints < $scope.RedeemPoints) {
            var text = angularService.getString("MaxPointsPrompt", $rootScope.languagecode);
            alert(text);
        }
        else {

            var MobileNo = "";
            var ccode = $scope.selectedcountrycodeobject;

            if ($scope.CustPhoneNumber != undefined) {
                if ($scope.CustPhoneNumber != "") {
                    MobileNo = ccode.CountryCode + " " + $scope.CustPhoneNumber;
                }
            }

            if (MobileNo == "") {
                if ($scope.Email != undefined) {
                    if ($scope.Email != "") {
                        MobileNo = $scope.Email;
                    }
                }
            }


            var getData = angularService.CalculatePointToRs($scope.RedeemPoints, MobileNo);
            getData.then(function (msg) {

                $scope.validitymsg = msg.data;


                $rootScope.divShowMessage = true;

                $rootScope.divRedeemCoupon = false;
                $rootScope.divConsumerProfile = false;
                $rootScope.divRedeemPoints = false;

            }, function (errdata) {
                if (errdata != null) {
                    if (errdata.data != null) {
                        alert('Error in redeem points');
                    }
                }
            });
        }
    };

    $scope.GetCouponFromScanner = function () {
        if ($("#hdnCouponCode").val() == undefined) {
            var text = angularService.getString("CouponCodePrompt", $rootScope.languagecode);
            alert(text);
        }
        else if ($scope.CustPhoneNumber == undefined) {
            var text = angularService.getString("MobileNoPrompt", $rootScope.languagecode);
            alert(text);
        }
        else if ($scope.BillAmount == undefined) {
            var text = angularService.getString("BillAmountPrompt", $rootScope.languagecode);
            alert(text);
        }
        else {
            $scope.CouponCode = $("#hdnCouponCode").val();
            var getData = angularService.CheckCouonValidity($scope.CouponCode, $scope.BillAmount, $scope.CustPhoneNumber);
            getData.then(function (msg) {

                if (String(msg.data).indexOf("Coupon is Valid.") >= 0) {
                    $scope.divRedeemCoupon = false;
                    $scope.validitymsg = msg.data;
                    $scope.divShowMessage = true;
                }
                else {
                    $scope.divRedeemCoupon = false;
                    $scope.validitymsg = "";
                    $scope.divShowMessage = false;
                    alert(msg.data);
                }
            }, function () {
                alert('Error in updating coupon');
            });
        }
    };

    $scope.sendCouponOTP = function () {
        var SharedCouponId = document.getElementsByName('hdnSharedCouponId')[0].value;

        var getData = angularService.sendOTPForCoupon(SharedCouponId);
        getData.then(function (msg) {
            var SendOTPMessage = angularService.getString("SendOTPMessage", $rootScope.languagecode);
            if (msg.data == "OTP Sent Successfully.") {
                $scope.divShowCouponOTP = true;
                $scope.divVerifiedCouponOTP = false;
            }
            else {
                alert(msg.data);
            }
        }, function (err) {
            $scope.divShowCouponOTP = false;
            $scope.divVerifiedCouponOTP = false;
            if (err != null) {
                if (err.data != null) {
                    alert(err.data);
                    alert('Error in sendCouponOTP ');
                }
            }
        });
    }



    $scope.VerifyCouponOTP = function () {

        if ($scope.couponotp != "") {
            var SharedCouponId = document.getElementsByName('hdnSharedCouponId')[0].value;

            var getData = angularService.VerifyCouponOTP($scope.couponotp, SharedCouponId);

            getData.then(function (msg) {
                var OTPVerified = angularService.getString("OTPVerified", $rootScope.languagecode);
                if (msg.data == OTPVerified) {
                    alert(msg.data);
                    $scope.divShowCouponOTP = false;
                    $scope.divVerifiedCouponOTP = true;
                }
                else {
                    alert(msg.data);
                    $scope.divShowCouponOTP = false;
                    $scope.divVerifiedCouponOTP = false;
                }
            }, function (err) {
                $scope.divShowCouponOTP = false;
                $scope.divVerifiedCouponOTP = false;
                if (err != null) {
                    if (err.data != null) {
                        alert(err.data);
                        alert('Error in VerifyCouponOTP');
                    }
                }
            });
        }
    }

    $scope.RedeemNow = function () {

        var CouponId = document.getElementsByName('hdnCouponId')[0].value;
        var SharedCouponId = document.getElementsByName('hdnSharedCouponId')[0].value;


        var getData = angularService.FinalRedeem(CouponId, SharedCouponId);
        getData.then(function (msg) {
            alert(msg.data);
            window.location.href = '/Merchant/ScanQRCode';
        }, function () {
            alert('Error in updating coupon');
        });
        //}
    };

    $scope.RedeemGiftcardNow = function () {
        var Id = document.getElementsByName('hdnId')[0].value;

        var getData = angularService.FinalGiftCardRedeem(Id);
        getData.then(function (msg) {
            alert(msg.data);
            window.location.href = '/Merchant/ScanQRCode';
        }, function () {
            alert('Error in updating coupon');
        });
        //}
    };


    $scope.SendOTPToConsumer = function () {
        var Id = document.getElementsByName('hdnId')[0].value;
        var getData = angularService.FinalGiftCardRedeem(Id);
        getData.then(function (msg) {
            alert(msg.data);
            window.location.href = '/Merchant/ScanQRCode';
        }, function () {
            alert('Error in updating coupon');
        });
        //}
    };

    $scope.RedeemPointsNow = function () {

        if ($scope.RedeemPoints == undefined) {
            var text = angularService.getString("EnterPointsPrompt", $rootScope.languagecode);
            alert(text);
        }
        else if ($scope.CustPhoneNumber == undefined && $scope.Email == undefined) {
            var text = angularService.getString("MobileNoEmailPrompt", $rootScope.languagecode);
            alert(text);
        }
        else if ($scope.CustPhoneNumber == "" && $scope.Email == "") {
            var text = angularService.getString("MobileNoEmailPrompt", $rootScope.languagecode);
            alert(text);
        }
        else if ($scope.CustPhoneNumber == "" && $scope.Email == undefined) {
            var text = angularService.getString("MobileNoEmailPrompt", $rootScope.languagecode);
            alert(text);
        }
        else if ($scope.CustPhoneNumber == undefined && $scope.Email == "") {
            var text = angularService.getString("MobileNoEmailPrompt", $rootScope.languagecode);
            alert(text);
        }
        else {
            var MobileNo = "";
            var ccode = $scope.selectedcountrycodeobject;

            if ($scope.CustPhoneNumber != undefined) {
                if ($scope.CustPhoneNumber != "") {
                    MobileNo = ccode.CountryCode + " " + $scope.CustPhoneNumber;
                }
            }

            if (MobileNo == "") {
                if ($scope.Email != undefined) {
                    if ($scope.Email != "") {
                        MobileNo = $scope.Email;
                    }
                }
            }

            var getData = angularService.FinalRedeemPoints($scope.RedeemPoints, MobileNo);
            getData.then(function (msg) {

                alert(msg.data);
                $scope.divRedeemCoupon = false;
                $scope.NoOfPoints = $scope.NoOfPoints - $scope.RedeemPoints;
                $scope.divConsumerProfile = true;
                $scope.validitymsg = "";
                $scope.divShowMessage = false;
            }, function (errdata) {
                if (errdata != null) {
                    if (errdata.data != null) {
                        alert('Error in redeem points');
                        $scope.divRedeemCoupon = false;
                        $scope.divConsumerProfile = true;
                        $scope.validitymsg = "";
                        $scope.divShowMessage = false;
                    }
                }
            });
        }
    };

    //////////////////////////////////////////////////
    //SET GIFT CARD
    //////////////////////////////////////////////////
    $scope.SetGiftCard = function (form, conditionelemnt) {

        if ($scope[form].$valid) {
            var giftcard = {
                Denomination1: $scope.Denomination1,
                Denom1DEC: $scope.denom1decdata,
                Denomination2: $scope.Denomination2,
                Denom2DEC: $scope.denom2decdata,
                Denomination3: $scope.Denomination3,
                Denom3DEC: $scope.denom3decdata,
                Denomination4: $scope.Denomination4,
                Denom4DEC: $scope.denom4decdata,
                Email: $scope.Email,
                TermsCondition: $scope.TermsCondition
            };
            //Add GiftCard
            var getData = angularService.SaveMerchantGiftCard(giftcard);
            getData.then(function (msg) {
                //alert(msg.data);
                saveGiftCardConditions(conditionelemnt)
                ClearGiftcardsFields();
            }, function () {
                alert('Error in Sending GiftCard');
            });

            function saveGiftCardConditions(conditionelemnt) {

                //Adding Conditons
                var getData = angularService.SaveGiftCardConditions(conditionelemnt);
                getData.then(function (msg) {
                    alert(msg.data);
                    ClearGiftcardsFields();
                }, function () {
                    alert('Error in Sending GiftCard');
                });
            }

        } else {
            $scope.showMsgs = true;
        }
    };

    function ClearGiftcardsFields() {
        $scope.Denomination1 = "";
        $scope.Denomination2 = "";
        $scope.Denomination3 = "";
        $scope.Denomination4 = "";
        $scope.Email = "";
        $scope.TermsConditon = "";
    }


    //******************************************//

    //Fill country, state, city, location
    /***********************************************************************************************************************/
    function fillCityList() {
        var getData = angularService.getCities();
        getData.then(function (cty) {
            $scope.CityList = cty.data;
        }, function () {
            alert('Error in getting records');
        });
    }


    function fillCountry() {

        var getData = angularService.getCountries();
        getData.then(function (country) {
            $scope.CountryList = country.data;
        }, function (errdata) {
            if (errdata != null) {
                if (errdata.data != null) {
                    alert(errdata.data);
                    alert('Error in getting records getCountry');
                }
            }
        });
    }

    $scope.getStates = function (country) {
        if (country != null) {
            $scope.countryid = country.countryid;
            var getData = angularService.getStates(country.countryid);
            getData.then(function (st) {
                $scope.StateList = st.data;
            }, function (errdata) {
                if (errdata != null) {
                    if (errdata.data != null) {
                        alert(errdata.data);
                        alert('Error in getting records getStates');
                    }
                }
            });
        }
        else {
            $scope.stateid = 0;
            $scope.locationid = 0;
            $scope.cityid = 0;
            $scope.LocationList = null;
            $scope.StateList = null;
        }
    }

    $scope.getCities = function (state) {
        if (state != null) {
            $scope.stateid = state.stateid;
            //var getData = angularService.getCities(state.stateid);
            //getData.then(function (city) {
            //    $scope.CityList = city.data;
            //}, function (errdata) {
            //    if (errdata != null) {
            //        alert(errdata.data);
            //        alert('Error in getting records getCities');
            //    }
            //});
        }
        else {
            $scope.stateid = 0;
            //$scope.locationid = 0;
            //$scope.cityid = 0;
            //$scope.LocationList = null;
            $scope.StateList = null;
            //$scope.CityList = null;
        }
    }

    $scope.setPeriod = function (exp) {
        
        if (exp != null) {
            $scope.expid = exp.Id;
        }
        else {
            $scope.expid = 0;
        }
    }

    $scope.getLocations = function (city) {

        if (city != null) {
            $scope.cityid = city.cityid;
            var getData = angularService.getLocations(city.cityid);
            getData.then(function (loc) {
                $scope.LocationList = loc.data;
            }, function (errdata) {
                if (errdata != null) {
                    alert(errdata.data);
                    alert('Error in getting records getLocations');
                }
            });
        }
        else {
            $scope.locationid = 0;
            $scope.cityid = 0;
            $scope.LocationList = null;
        }
    }


    $scope.LocationChanged = function (loc) {
        if (loc != null) {
            $scope.locationid = loc.LocationId;
        }
    }

    $scope.CategoryChanged = function (cat) {
        if (cat != null) {
            $scope.categoryid = cat.categoryid;
        }
    }

    $scope.BrandChanged = function (bnd) {
        if (bnd != null) {
            $scope.BrandId = bnd.BrandId;
            $scope.BranchList = bnd.branches;
        }
    }

    $scope.BranchChanged = function (bch) {
        if (bch != null) {
            $scope.BranchId = bch.BranchId;
        }
    }
    /***********************************************************************************************************************/

});

var directiveModule = angular.module('angularjs-dropdown-multiselect', []);

directiveModule.directive('ngDropdownMultiselect', ['$filter', '$document', '$compile', '$parse',

function ($filter, $document, $compile, $parse) {

    return {
        restrict: 'AE',
        scope: {
            selectedModel: '=',
            options: '=',
            extraSettings: '=',
            events: '=',
            searchFilter: '=?',
            translationTexts: '=',
            groupBy: '@'
        },
        template: function (element, attrs) {
            var checkboxes = attrs.checkboxes ? true : false;
            var groups = attrs.groupBy ? true : false;

            var template = '<div class="multiselect-parent btn-group dropdown-multiselect">';
            template += '<button type="button" class="dropdown-toggle" ng-class="settings.buttonClasses" ng-click="toggleDropdown()">{{getButtonText()}}&nbsp;<span class="caret"></span></button>';
            template += '<ul class="dropdown-menu dropdown-menu-form" ng-style="{display: open ? \'block\' : \'none\', height : settings.scrollable ? settings.scrollableHeight : \'auto\' }" style="overflow: scroll" >';
            template += '<li ng-hide="!settings.showCheckAll || settings.selectionLimit > 0"><a data-ng-click="selectAll()"><span class="glyphicon glyphicon-ok"></span>  {{texts.checkAll}}</a>';
            template += '<li ng-hide="(!settings.showCheckAll || settings.selectionLimit > 0) && !settings.showUncheckAll" class="divider"></li>';
            template += '<li ng-show="settings.enableSearch"><div class="dropdown-header"><input type="text" class="form-control" style="width: 100%;" ng-model="searchFilter" placeholder="{{texts.searchPlaceholder}}" /></li>';
            template += '<li ng-show="settings.enableSearch" class="divider"></li>';

            if (groups) {
                template += '<li ng-repeat-start="option in orderedItems | filter: searchFilter" ng-show="getPropertyForObject(option, settings.groupBy) !== getPropertyForObject(orderedItems[$index - 1], settings.groupBy)" role="presentation" class="dropdown-header">{{ getGroupTitle(getPropertyForObject(option, settings.groupBy)) }}</li>';
                template += '<li ng-repeat-end role="presentation">';
            } else {
                template += '<li role="presentation" ng-repeat="option in options | filter: searchFilter">';
            }

            template += '<a role="menuitem" tabindex="-1" ng-click="setSelectedItem(getPropertyForObject(option,settings.idProp))">';

            if (checkboxes) {
                template += '<div class="checkbox"><label><input class="checkboxInput" type="checkbox" ng-click="checkboxClick($event, getPropertyForObject(option,settings.idProp))" ng-checked="isChecked(getPropertyForObject(option,settings.idProp))" /> {{getPropertyForObject(option, settings.displayProp)}}</label></div></a>';
            } else {
                template += '<span data-ng-class="{\'glyphicon glyphicon-ok\': isChecked(getPropertyForObject(option,settings.idProp))}"></span> {{getPropertyForObject(option, settings.displayProp)}}</a>';
            }

            template += '</li>';

            template += '<li class="divider" ng-show="settings.selectionLimit > 1"></li>';
            template += '<li role="presentation" ng-show="settings.selectionLimit > 1"><a role="menuitem">{{selectedModel.length}} {{texts.selectionOf}} {{settings.selectionLimit}} {{texts.selectionCount}}</a></li>';

            template += '</ul>';
            template += '</div>';

            element.html(template);
        },
        link: function ($scope, $element, $attrs) {
            var $dropdownTrigger = $element.children()[0];

            $scope.toggleDropdown = function () {
                $scope.open = !$scope.open;
            };

            $scope.checkboxClick = function ($event, id) {
                $scope.setSelectedItem(id);
                $event.stopImmediatePropagation();
            };

            $scope.externalEvents = {
                onItemSelect: angular.noop,
                onItemDeselect: angular.noop,
                onSelectAll: angular.noop,
                onDeselectAll: angular.noop,
                onInitDone: angular.noop,
                onMaxSelectionReached: angular.noop
            };

            $scope.settings = {
                dynamicTitle: true,
                scrollable: false,
                scrollableHeight: '300px',
                closeOnBlur: true,
                displayProp: 'label',
                idProp: 'id',
                externalIdProp: 'id',
                enableSearch: false,
                selectionLimit: 0,
                showCheckAll: true,
                showUncheckAll: true,
                closeOnSelect: false,
                buttonClasses: 'btn btn-default',
                closeOnDeselect: false,
                groupBy: $attrs.groupBy || undefined,
                groupByTextProvider: null,
                smartButtonMaxItems: 0,
                smartButtonTextConverter: angular.noop
            };

            $scope.texts = {
                checkAll: 'Check All',
                uncheckAll: 'Uncheck All',
                selectionCount: 'checked',
                selectionOf: '/',
                searchPlaceholder: 'Search...',
                buttonDefaultText: 'Select',
                dynamicButtonTextSuffix: 'checked'
            };

            $scope.searchFilter = $scope.searchFilter || '';

            if (angular.isDefined($scope.settings.groupBy)) {
                $scope.$watch('options', function (newValue) {
                    if (angular.isDefined(newValue)) {
                        $scope.orderedItems = $filter('orderBy')(newValue, $scope.settings.groupBy);
                    }
                });
            }

            angular.extend($scope.settings, $scope.extraSettings || []);
            angular.extend($scope.externalEvents, $scope.events || []);
            angular.extend($scope.texts, $scope.translationTexts);

            $scope.singleSelection = $scope.settings.selectionLimit === 1;

            function getFindObj(id) {
                var findObj = {};

                if ($scope.settings.externalIdProp === '') {
                    findObj[$scope.settings.idProp] = id;
                } else {
                    findObj[$scope.settings.externalIdProp] = id;
                }

                return findObj;
            }

            function clearObject(object) {
                for (var prop in object) {
                    delete object[prop];
                }
            }

            if ($scope.singleSelection) {
                if (angular.isArray($scope.selectedModel) && $scope.selectedModel.length === 0) {
                    clearObject($scope.selectedModel);
                }
            }

            if ($scope.settings.closeOnBlur) {
                $document.on('click', function (e) {
                    var target = e.target.parentElement;
                    var parentFound = false;

                    while (angular.isDefined(target) && target !== null && !parentFound) {
                        if (_.contains(target.className.split(' '), 'multiselect-parent') && !parentFound) {
                            if (target === $dropdownTrigger) {
                                parentFound = true;
                            }
                        }
                        target = target.parentElement;
                    }

                    if (!parentFound) {
                        $scope.$apply(function () {
                            $scope.open = false;
                        });
                    }
                });
            }

            $scope.getGroupTitle = function (groupValue) {
                if ($scope.settings.groupByTextProvider !== null) {
                    return $scope.settings.groupByTextProvider(groupValue);
                }

                return groupValue;
            };

            $scope.getButtonText = function () {
                if ($scope.settings.dynamicTitle && ($scope.selectedModel.length > 0 || (angular.isObject($scope.selectedModel) && _.keys($scope.selectedModel).length > 0))) {
                    if ($scope.settings.smartButtonMaxItems > 0) {
                        var itemsText = [];

                        angular.forEach($scope.options, function (optionItem) {
                            if ($scope.isChecked($scope.getPropertyForObject(optionItem, $scope.settings.idProp))) {
                                var displayText = $scope.getPropertyForObject(optionItem, $scope.settings.displayProp);
                                var converterResponse = $scope.settings.smartButtonTextConverter(displayText, optionItem);

                                itemsText.push(converterResponse ? converterResponse : displayText);
                            }
                        });

                        if ($scope.selectedModel.length > $scope.settings.smartButtonMaxItems) {
                            itemsText = itemsText.slice(0, $scope.settings.smartButtonMaxItems);
                            itemsText.push('...');
                        }

                        return itemsText.join(', ');
                    } else {
                        var totalSelected;

                        if ($scope.singleSelection) {
                            totalSelected = ($scope.selectedModel !== null && angular.isDefined($scope.selectedModel[$scope.settings.idProp])) ? 1 : 0;
                        } else {
                            totalSelected = angular.isDefined($scope.selectedModel) ? $scope.selectedModel.length : 0;
                        }

                        if (totalSelected === 0) {
                            return $scope.texts.buttonDefaultText;
                        } else {
                            return totalSelected + ' ' + $scope.texts.dynamicButtonTextSuffix;
                        }
                    }
                } else {
                    return $scope.texts.buttonDefaultText;
                }
            };

            $scope.getPropertyForObject = function (object, property) {
                if (angular.isDefined(object) && object.hasOwnProperty(property)) {
                    return object[property];
                }

                return '';
            };

            $scope.selectAll = function () {
                $scope.deselectAll(false);
                $scope.externalEvents.onSelectAll();

                angular.forEach($scope.options, function (value) {
                    $scope.setSelectedItem(value[$scope.settings.idProp], true);
                });
            };

            $scope.deselectAll = function (sendEvent) {
                sendEvent = sendEvent || true;

                if (sendEvent) {
                    $scope.externalEvents.onDeselectAll();
                }

                if ($scope.singleSelection) {
                    clearObject($scope.selectedModel);
                } else {
                    $scope.selectedModel.splice(0, $scope.selectedModel.length);
                }
            };

            $scope.setSelectedItem = function (id, dontRemove) {
                var findObj = getFindObj(id);
                var finalObj = null;

                if ($scope.settings.externalIdProp === '') {
                    finalObj = _.find($scope.options, findObj);
                } else {
                    finalObj = findObj;
                }

                if ($scope.singleSelection) {
                    clearObject($scope.selectedModel);
                    angular.extend($scope.selectedModel, finalObj);
                    $scope.externalEvents.onItemSelect(finalObj);
                    if ($scope.settings.closeOnSelect) $scope.open = false;

                    return;
                }

                dontRemove = dontRemove || false;

                var exists = _.findIndex($scope.selectedModel, findObj) !== -1;

                if (!dontRemove && exists) {
                    $scope.selectedModel.splice(_.findIndex($scope.selectedModel, findObj), 1);
                    $scope.externalEvents.onItemDeselect(findObj);
                } else if (!exists && ($scope.settings.selectionLimit === 0 || $scope.selectedModel.length < $scope.settings.selectionLimit)) {
                    $scope.selectedModel.push(finalObj);
                    $scope.externalEvents.onItemSelect(finalObj);
                }
                if ($scope.settings.closeOnSelect) $scope.open = false;
            };

            $scope.isChecked = function (id) {
                if ($scope.singleSelection) {
                    return $scope.selectedModel !== null && angular.isDefined($scope.selectedModel[$scope.settings.idProp]) && $scope.selectedModel[$scope.settings.idProp] === getFindObj(id)[$scope.settings.idProp];
                }

                return _.findIndex($scope.selectedModel, getFindObj(id)) !== -1;
            };

            $scope.externalEvents.onInitDone();
        }


    };
}]);