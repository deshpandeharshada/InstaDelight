
app.controller("myCntrl", function ($scope, $rootScope, angularService) {

    $rootScope.searchButtonText = false;

    $rootScope.showFooter = true;

    //$scope.currentPage = 0;
    //$scope.pageSize = 5;
    //$scope.datacoupon = [];
    $rootScope.languagecode = "en";

    $rootScope.ShowCurrency = false;
    $scope.divShowPayButton = false;

    $rootScope.Discount = 0;

    $scope.setLanguageCode = function () {
        getLanguageCode();
    }

    $scope.openGetNewDECForm = function () {
        getLanguageCode();
        fillCountry();
        $scope.selectedcountrycodeobject = { countryid: 1, CountryCode: "+91" };
        $scope.selectedMerchantcountrycodeobject = { countryid: 1, CountryCode: "+91" };
    }

    $scope.OpenBuyGiftCardForm = function () {
        var flag = document.getElementsByName('hdnFlag')[0].value;
        if (flag == "show Pay Button") {
            $scope.divShowPayButton = true;
        }
        else {
            $scope.divShowPayButton = false;
        }
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

    $scope.CountryList = null;
    $scope.StateList = null;
    $scope.CityList = null;
    $scope.LocationList = null;
    $scope.CategoryList = null;

    $scope.locationid = 0;
    $scope.cityid = 0;
    $scope.countryid = 0;
    $scope.stateid = 0;

    $scope.Points = 0;

    $scope.categoryid = 0;
    $scope.bankid = 0;
    $scope.MerchantId = 0;

    $scope.Question1 = "";
    $scope.Question2 = "";
    $scope.Question3 = "";
    $scope.Question4 = "";
    $scope.DefaultQuestion = "";

    $scope.lblCountryCode = "";
    $scope.currency = "";

    $scope.b64encoded = 'none';
    $scope.b64banklogoencoded = 'none';
    $scope.b64decencoded = 'none';
    $scope.b64merdecencoded = 'none';
    $scope.b64merlogoencoded = 'none';

    $scope.divShowEmailOTP = false;

    $scope.coupondecdata = null;
    // $scope.couponqrcodedata = null;

    $scope.genders = ["Female", "Male"];

    $scope.EnableHomeBackButton = function () {
        $rootScope.showFooter = true;
    }

    $scope.numRows = 5;
    $scope.gridOptions = {
        columnDefs: [
         { name: 'CouponTitle', headerCellTemplate: '<div></div>', cellClass: 'htabGrid', width: "*", resizable: false, cellTemplate: '<div ng-click="grid.appScope.showCoupon(row.entity)">{{row.entity.CouponTitle}}</div>' },
         { name: 'MerchantId', headerCellTemplate: '<div></div>', visible: false },
          { name: 'couponid', cellTemplate: '<a href="#">{{ row.entity.Id }}</a>', headerCellTemplate: '<div></div>', visible: false },
          { name: 'Share', cellClass: 'htabGridshare small-2 columns', cellTemplate: ' <div style="float:right;margin-right: 4px;"> <a href="/Consumer/SendCoupon?MerchantId={{row.entity.MerchantId}}&CouponId={{row.entity.couponid}}&SharedCouponId={{row.entity.Id}}"><i class="fa fa-share-alt fa-2x red-icon" aria-hidden="true"></i></a>  </div>', headerCellTemplate: '<div></div>', width: "15%", resizable: false }
        ],
        enableColumnResize: false,
        rowHeight: 65,
        paginationPageSize: $scope.numRows,
        paginationPageSizes: [5, 10, 50, 100],
        enablePaginationControls: true,
        paginationCurrentPage: 1,
    };

    $scope.gridOptions.onRegisterApi = function (gridApi) {
        $scope.gridApi2 = gridApi;
    }

    $scope.verifyEmailId = function () {
        if ($scope.Email != "") {
            var getData = angularService.sendOTPtoEmail($scope.Email);
            getData.then(function (msg) {
                var SendOTPMessage = angularService.getString("SendOTPMessage", $rootScope.languagecode);

                if (msg.data == SendOTPMessage) {
                    $scope.divShowEmailOTP = true;
                }
                else {
                    alert(msg.data);
                }
            }, function (err) {

                $scope.divShowEmailOTP = false;
                if (err != null) {
                    if (err.data != null) {
                        alert(err.data);
                        alert('Error in verifyEmailId');
                    }
                }
            });
        }
    }

    $scope.VerifyEmailOTP = function () {

        if ($scope.emailotp != "") {
            var getData = angularService.VerifyEmailOTP($scope.emailotp);
            getData.then(function (msg) {
                var OTPVerified = angularService.getString("OTPVerified", $rootScope.languagecode);
                if (msg.data == OTPVerified) {
                    $scope.EmailVerified = true;
                    $scope.divShowEmailOTP = false;
                }
                else {
                    alert(msg.data);
                    $scope.divShowEmailOTP = false;
                }
            }, function (err) {
                $scope.divShowEmailOTP = false;
                if (err != null) {
                    if (err.data != null) {
                        alert(err.data);
                        alert('Error in VerifyEmailOTP');
                    }
                }
            });
        }
    }

    $scope.verifyPhone2 = function () {
        if ($scope.Phone2 != "") {
            var getData = angularService.sendOTPtoPhone2($scope.Phone2);
            getData.then(function (msg) {
                var SendOTPMessage = angularService.getString("SendOTPMessage", $rootScope.languagecode);
                if (msg.data == SendOTPMessage) {
                    $scope.divShowPhone2OTP = true;
                }
                else {
                    alert(msg.data);
                }
            }, function (err) {
                $scope.divShowPhone2OTP = false;
                if (err != null) {
                    if (err.data != null) {
                        alert(err.data);
                        alert('Error in verifyPhone2');
                    }
                }
            });
        }
    }

    $scope.Verifyphone2otp = function () {

        if ($scope.phone2otp != "") {
            var getData = angularService.VerifyPhone2OTP($scope.phone2otp);
            getData.then(function (msg) {
                var OTPVerified = angularService.getString("OTPVerified", $rootScope.languagecode);
                if (msg.data == OTPVerified) {
                    $scope.Phone2Verified = true;
                    $scope.divShowPhone2OTP = false;
                }
                else {
                    alert(msg.data);
                    $scope.divShowPhone2OTP = false;
                }
            }, function (err) {
                $scope.divShowPhone2OTP = false;
                if (err != null) {
                    if (err.data != null) {
                        alert(err.data);
                        alert('Error in Verifyphone2otp');
                    }
                }
            });
        }
    }

    $scope.verifyPhone3 = function () {
        if ($scope.Phone3 != "") {
            var getData = angularService.sendOTPtoPhone3($scope.Phone3);
            getData.then(function (msg) {
                var SendOTPMessage = angularService.getString("SendOTPMessage", $rootScope.languagecode);
                if (msg.data == SendOTPMessage) {
                    $scope.divShowPhone3OTP = true;
                }
                else {
                    alert(msg.data);
                }
            }, function (err) {
                $scope.divShowPhone3OTP = false;
                if (err != null) {
                    if (err.data != null) {
                        alert(err.data);
                        alert('Error in verifyPhone3');
                    }
                }
            });
        }
    }

    $scope.Verifyphone3otp = function () {

        if ($scope.phone3otp != "") {
            var getData = angularService.VerifyPhone3OTP($scope.phone3otp);

            getData.then(function (msg) {
                var OTPVerified = angularService.getString("OTPVerified", $rootScope.languagecode);
                if (msg.data == OTPVerified) {
                    $scope.Phone3Verified = true;
                    $scope.divShowPhone3OTP = false;
                }
                else {
                    alert(msg.data);
                    $scope.divShowPhone3OTP = false;
                }
            }, function (err) {
                $scope.divShowPhone2OTP = false;
                if (err != null) {
                    if (err.data != null) {
                        alert(err.data);
                        alert('Error in Verifyphone3otp');
                    }
                }
            });
        }
    }


    $scope.setImage = function (gender) {
        if (gender != null) {
            var result = '';
            if (gender == 'Male') {
                var url = [location.origin + "/Images/male_user_icon.png"];
                var canvas = document.createElement('CANVAS');
                img = document.createElement('img'),
                img.src = url;
                img.onload = function () {

                    canvas.height = img.height;
                    canvas.width = img.width;
                    var dataURL = canvas.toDataURL('image/png');
                    canvas = null;
                    $scope.b64encoded = url;
                    $scope.consumerlogodata = btoa($scope.b64encoded);
                };
            }
            else if (gender == 'Female') {
                var url = [location.origin + "/Images/female_user_icon.png"];
                var canvas = document.createElement('CANVAS');
                img = document.createElement('img'),
                img.src = url;
                img.onload = function () {

                    canvas.height = img.height;
                    canvas.width = img.width;
                    var dataURL = canvas.toDataURL('image/png');
                    canvas = null;
                    $scope.b64encoded = url;
                    $scope.consumerlogodata = btoa($scope.b64encoded);
                };
            }
        }
    }

    $scope.OpenForm = function () {
        getLanguageCode();
        GetAllBanks();
        GetAllMerchants();
        GetCountryCode();
        $rootScope.divDEC = false;
        $rootScope.divCoupons = false;
        $rootScope.divBanks = true;
        $rootScope.divCouponDetails = false;
        $rootScope.divMerchantDEC = false;

    }

    $scope.OpenMerchantReviewForm = function () {
        getLanguageCode();
        $rootScope.showFooter = true;
        GetAllReview();
    }

    $scope.OpenHomeScreen = function () {



        $rootScope.showFooter = false;
        //var getData = angularService.getallReview();
        var getData = angularService.getpendingReview();
        getData.then(function (mch) {
            $scope.noofreviews = mch.data;
        }, function (err) {
            if (err != null) {
                if (err.data != null) {
                    alert(err.data);
                    alert('Error in getting records OpenHomeScreen');
                }
            }
        });
    }



    // TO Get Merchant Review Details
    function GetAllReview() {
        var getData = angularService.getallReview();
        getData.then(function (mch) {
            $scope.merchants = mch.data;
        }, function (errdata) {
            if (errdata != null) {
                if (errdata.data != null) {
                    alert(errdata.data);
                    alert('Error in getting records GetAllReview');
                }
            }
        });
    }


    $scope.showMerchantReviews = function (merchantid) {

        $scope.MerchantId = merchantid;
        GetAllMerchantReviews($scope.MerchantId);
    }

    function GetAllMerchantReviews(merchantid) {
        location.href = "/Consumer/PendingReviews/" + merchantid;
    }

    $scope.OpenProfileForm = function () {
        getLanguageCode();
        $rootScope.showFooter = true;
        fillCountry();
        editConsumer();
        GetCountryCode();
    }

    $scope.openSendCoupon = function () {

        getLanguageCode();
        $rootScope.showFooter = true;

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
                                alert('Error in getting records editconsumer/getcountrycode');
                            }
                        }

                    });
                }
            }
        }
    }

    function GetCountryCode() {
        var getData = angularService.getCountrycode();
        getData.then(function (ctry) {

            $scope.lblCountryCode = ctry.data.CountryCode;
            $scope.currency = ctry.data.currency;
            if (ctry.data.currency == 'Rs') {
                $scope.ShowCurrency = true;
            }
            else {
                $scope.ShowCurrency = false;
            }
        }, function (errdata) {
            if (errdata != null) {
                if (errdata.data != null) {
                    alert(errdata.data);
                    alert('Error in getting records GetCountryCode');
                }
            }
        });
    }

    $scope.OpenReviewForm = function () {
        getLanguageCode();
        $rootScope.showFooter = true;
        GetPendingReviewForMerchant();
    }

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

    $scope.GetConsumerGiftCards = function () {
        $rootScope.showFooter = true;
        $rootScope.divPurchasedCards = true;
        GetConsumerGiftCards();
    }

    $scope.getCountryId = function (country) {
        if (country != null) {
            $scope.lblCountryId = country.countryid;
        }
    }

    function GetPendingReviewForMerchant() {

        var merchantId = document.getElementsByName('hdnMerchantId')[0].value;

        var getData = angularService.GetPendingReviewForMerchant(merchantId);


        getData.then(function (rev) {

            $scope.review = rev.data[0];
            $scope.reviewid = rev.data[0].reviewid;
            $scope.Question1 = rev.data[0].Question1;
            $scope.Question2 = rev.data[0].Question2;
            $scope.Question3 = rev.data[0].Question3;
            $scope.Question4 = rev.data[0].Question4;
            $scope.DefaultQuestion = rev.data[0].DefaultQuestion;

        }, function (err) {
            if (err != null) {
                if (err.data != null) {
                    alert(err.data);
                    alert('Error in getting records GetPendingReviewForMerchant');
                }
            }

        });
    }



    $scope.SaveChangedCountry = function () {
        var countryid = $scope.lblCountryId;
        var langid = $scope.languageid;
        var getData = angularService.SaveChangedCountry(countryid, langid);
        getData.then(function (msg) {
            getLanguageCode();
            alert(msg.data);
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

    //Get Language
    function getLanguage(country) {

        if (country != null) {
            $scope.countryid = country.countryid;
            $scope.lblCountryId = country.countryid;
            var getData = angularService.getLanguage(country.countryid);
            getData.then(function (loc) {
                $scope.LanguageList = loc.data;
            }, function (errdata) {
                if (errdata != null) {
                    if (errdata.data != null) {
                        alert(errdata.data);
                        alert('Error in getting records getLanguage');
                    }
                }
            });
        } else {
            $scope.languageid = 0;
            $scope.LanguageList = null;
        }
    }



    function editConsumer() {

        var Consumerid = document.getElementsByName('hdnConsumerId')[0].value;
        $scope.id = Consumerid;
        var getData = angularService.getConsumer(parseInt(Consumerid));
        getData.then(function (mch) {

            $scope.consumer = mch.data;
            $scope.id = mch.data.id;
            $scope.UserId = mch.data.UserId;
            $scope.Name = mch.data.consumername;
            $scope.gender = mch.data.Gender;
            $scope.BuildingName = mch.data.BuildingName;
            $scope.SocietyName = mch.data.SocietyName;
            $scope.Street = mch.data.Street;
            $scope.locationid = mch.data.Location;
            $scope.countryid = mch.data.Country;
            $scope.stateid = mch.data.State;
            $scope.cityid = mch.data.City;

            $scope.City = mch.data.City;
            $scope.Location = mch.data.Location;


            $scope.PinCode = mch.data.PinCode;
            $scope.Email = mch.data.Email;
            $scope.EmailVerified = mch.data.EmailVerified;

            $scope.Phone1 = mch.data.Phone1;
            $scope.Phone2 = mch.data.Phone2;
            $scope.Phone2Verified = mch.data.Phone2Verified;

            $scope.Phone3 = mch.data.Phone3;
            $scope.Phone3Verified = mch.data.Phone3Verified;

            if (mch.data.DOA != null) {
                $scope.DOA = mch.data.DOA;
                $scope.DOA = $scope.DOA.replace('/Date(', '');
                $scope.DOA = $scope.DOA.replace(')/', '');
                $scope.DOA = new Date(parseInt($scope.DOA));
            }

            if (mch.data.DOB != null) {
                $scope.DOB = mch.data.DOB;

                $scope.DOB = $scope.DOB.replace('/Date(', '');

                $scope.DOB = $scope.DOB.replace(')/', '');
                $scope.DOB = new Date(parseInt($scope.DOB));
            }

            if (mch.data.consumerlogo != null) {
                var CHUNK_SIZE = 0x8000; // arbitrary number here, not too small, not too big
                var index = 0;
                var length = mch.data.consumerlogo.length;
                var result = '';
                var slice;
                while (index < length) {
                    slice = mch.data.consumerlogo.slice(index, Math.min(index + CHUNK_SIZE, length)); // `Math.min` is not really necessary here I think
                    result += String.fromCharCode.apply(null, slice);
                    index += CHUNK_SIZE;
                }
            }
            else {
                //----------------------------------------------------------------------------------------------------------------//
                //-PA
                //Added else part as when there is no cusomerlogo field is null in Db then default image will be set as per Gender
                //----------------------------------------------------------------------------------------------------------------//
                var result = '';
                if (mch.data.Gender == 'Male') {
                    var url = [location.origin + "/Images/male_user_icon.png"];
                    var canvas = document.createElement('CANVAS');
                    img = document.createElement('img'),
                    img.src = url;
                    img.onload = function () {
                        canvas.height = img.height;
                        canvas.width = img.width;
                        var dataURL = canvas.toDataURL('image/png');
                        canvas = null;
                        $scope.b64encoded = url;
                        $scope.consumerlogodata = btoa($scope.b64encoded);
                    };
                }
                else if (mch.data.Gender == 'Female') {

                    var url = [location.origin + "/Images/female_user_icon.png"];
                    var canvas = document.createElement('CANVAS');
                    img = document.createElement('img'),
                    img.src = url;
                    img.onload = function () {

                        canvas.height = img.height;
                        canvas.width = img.width;
                        var dataURL = canvas.toDataURL('image/png');
                        canvas = null;
                        $scope.b64encoded = url;
                        $scope.consumerlogodata = btoa($scope.b64encoded);
                    };
                }

            }

            $scope.b64encoded = result;//String.fromCharCode.apply(null, coupon.DEC);
            $scope.consumerlogodata = btoa($scope.b64encoded);

            $scope.selectedcountryobject = { countryid: $scope.countryid };

            if ($scope.Phone2 == undefined || $scope.Phone2 == "") {
                var getCountryData = angularService.getCountrycode();
                getCountryData.then(function (mch) {
                    $scope.selectedcountrycode2object = { countryid: $scope.countryid, CountryCode: mch.data.CountryCode };
                }, function (err) {
                    if (err != null) {
                        if (err.data != null) {
                            alert(err.data);
                            alert('Error in getting records editconsumer/getcountrycode');
                        }
                    }

                });
            }
            else {
                var array = $scope.Phone2.split(' ');
                if (array.length == 2) {
                    $scope.Phone2 = array[1];
                    var getCountryData = angularService.getCountryFromCountryCode(array[0]);
                    getCountryData.then(function (mch) {
                        $scope.selectedcountrycode2object = { countryid: mch.data.countryid, CountryCode: mch.data.CountryCode };
                    }, function (err) {
                        if (err != null) {
                            if (err.data != null) {
                                alert(err.data);
                                alert('Error in getting records editconsumer/getcountryfromcode');
                            }
                        }

                    });
                }
                else {
                    var getCountryData = angularService.getCountrycode();
                    getCountryData.then(function (mch) {
                        $scope.selectedcountrycode2object = { countryid: $scope.countryid, CountryCode: mch.data.CountryCode };
                    }, function (err) {
                        if (err != null) {
                            if (err.data != null) {
                                alert(err.data);
                                alert('Error in getting records editconsumer/getcountrycode');
                            }
                        }

                    });
                }
            }



            if ($scope.Phone3 == undefined || $scope.Phone3 == "") {
                var getCountryData = angularService.getCountrycode();
                getCountryData.then(function (mch) {
                    $scope.selectedcountrycode3object = { countryid: $scope.countryid, CountryCode: mch.data.CountryCode };
                }, function (err) {
                    if (err != null) {
                        if (err.data != null) {
                            alert(err.data);
                            alert('Error in getting records editconsumer/getcountrycode');
                        }
                    }

                });
            }
            else {
                var array = $scope.Phone3.split(' ');
                if (array.length == 2) {
                    $scope.Phone3 = array[1];
                    var getCountryData = angularService.getCountryFromCountryCode(array[0]);
                    getCountryData.then(function (mch) {
                        $scope.selectedcountrycode3object = { countryid: mch.data.countryid, CountryCode: mch.data.CountryCode };
                    }, function (err) {
                        if (err != null) {
                            if (err.data != null) {
                                alert(err.data);
                                alert('Error in getting records editconsumer/getcountryfromcode');
                            }
                        }

                    });
                }
                else {
                    var getCountryData = angularService.getCountrycode();
                    getCountryData.then(function (mch) {
                        $scope.selectedcountrycode3object = { countryid: $scope.countryid, CountryCode: mch.data.CountryCode };
                    }, function (err) {
                        if (err != null) {
                            if (err.data != null) {
                                alert(err.data);
                                alert('Error in getting records editconsumer/getcountrycode');
                            }
                        }

                    });
                }
            }


            $scope.selectedstateobject = { stateid: $scope.stateid };
            //$scope.selectedcityobject = { cityid: $scope.cityid };
            //$scope.selectedlocobject = { LocationId: $scope.locationid };

            $scope.getStates($scope.selectedcountryobject);
            //$scope.getCities($scope.selectedstateobject);
            //$scope.getLocations($scope.selectedcityobject);

        }, function (err) {
            if (err != null) {
                if (err.data != null) {
                    alert(err.data);
                    alert('Error in getting records editConsumer');
                }
            }

        });
    }

    //To Get All Records  
    function GetAllBanks() {

        var getData = angularService.GetAllBanks();
        getData.then(function (bnk) {

            $scope.banks = bnk.data;
            //$scope.b64encoded = btoa(String.fromCharCode.apply(null, bnk.data[0].bank_logo));

        }, function (errdata) {
            if (errdata != null) {
                if (errdata.data != null) {
                    alert(errdata.data);
                    alert('Error in getting records GetAllBanks');
                }
            }
        });
    }

    //To Get All Records  
    function GetAllMerchants() {

        var getData = angularService.getMerchants();
        getData.then(function (mch) {

            $scope.merchants = mch.data;
        }, function (errdata) {
            if (errdata != null) {
                if (errdata.data != null) {
                    alert(errdata.data);
                    alert('Error in getting records GetAllMerchants');
                }
            }
        });
    }


    //To Get All Records  
    function GetAllCoupons(cityid, locationid, categoryid, bankid) {
        // $rootScope.searchButtonText = true;
        $scope.loaderMore = true;
        $scope.lblMessage = 'loading please wait....!';
        $scope.result = "color-green";

        var getData = angularService.GetCoupons(cityid, locationid, categoryid, bankid);
        return getData.then(function (coupon) {

            $scope.gridOptions.excessRows = coupon.data.length;
            $scope.gridOptions.data = coupon.data;
            $scope.gridOptions.totalItems = coupon.data.length;
            if (coupon.data.length < $scope.numRows) {
                $scope.gridOptions.minRowsToShow = coupon.data.length;

            }
            else {
                $scope.gridOptions.minRowsToShow = $scope.numRows;
            }

            //$scope.coupons = coupon.data;
            $rootScope.divDEC = false;
            $rootScope.divBanks = false;
            $rootScope.divMerchantDEC = false;
            $rootScope.divCouponDetails = false;
            $rootScope.divCoupons = true;
            $rootScope.divBankCoupons = true;
            $rootScope.searchButtonText = false;
        }, function (errdata) {
            $rootScope.searchButtonText = false;
            $rootScope.divBanks = false;
            $rootScope.divMerchantDEC = false;
            $rootScope.divCouponDetails = false;
            $rootScope.divCoupons = false;
            $rootScope.divBankCoupons = false;
            $rootScope.divDEC = true;
            if (errdata != null) {
                if (errdata.data != null) {
                    alert(errdata.data);
                    alert('Error in getting records GetAllCoupons');
                }
            }
        });

    }

    //To Get All Records  
    function GetAllMerchantCoupons(merchantid) {

        var getData = angularService.GetMerchantCoupons(merchantid);
        getData.then(function (coupon) {
            if (coupon.data.length == 0) {

                $rootScope.divDEC = false;
                $rootScope.divBanks = false;
                $rootScope.divCoupons = false;
                $rootScope.divCouponDetails = false;
                $rootScope.divBankCoupons = false;
                $rootScope.divMerchantDEC = true;

                $scope.MerchantId = merchantid;
                var text = angularService.getString("NocouponsMessage", $rootScope.languagecode);
                alert(text);
            }
            else {

                $rootScope.divDEC = false;
                $rootScope.divBanks = false;
                $rootScope.divMerchantDEC = false;
                $rootScope.divCouponDetails = false;
                $rootScope.divBankCoupons = false;
                $rootScope.divCoupons = true;
                $scope.MerchantId = merchantid;
                $scope.coupons = coupon.data;

                $scope.gridOptions.excessRows = coupon.data.length;
                $scope.gridOptions.data = coupon.data;
                $scope.gridOptions.totalItems = coupon.data.length;
                if (coupon.data.length < $scope.numRows) {
                    $scope.gridOptions.minRowsToShow = coupon.data.length;
                }
                else {
                    $scope.gridOptions.minRowsToShow = $scope.numRows;
                }

            }
            $rootScope.searchButtonText = false;
        }, function (errdata) {

            $rootScope.searchButtonText = false;
            $rootScope.divDEC = false;
            $rootScope.divBanks = false;
            $rootScope.divMerchantDEC = false;
            $rootScope.divCouponDetails = false;
            $rootScope.divBankCoupons = false;

            $rootScope.divCoupons = true;
            if (errdata != null) {
                if (errdata.data != null) {
                    alert(errdata.data);
                    alert('Error in getting records GetAllMerchantCoupons');
                }
            }
        });
    }


    function fillBusinessCategoryList() {
        // $rootScope.searchButtonText = true;
        var getData = angularService.getBusinessCategory();
        return getData.then(function (busi) {
            $scope.CategoryList = busi.data;
            //   $rootScope.searchButtonText = false;
        }, function (errdata) {
            if (errdata != null) {
                $rootScope.searchButtonText = false;
                if (errdata.data != null) {
                    alert(errdata.data);
                    alert('Error in getting records fillBusinessCategoryList');
                }
            }
        });
    }

    $scope.getImageUrl = function (bnklogo) {
        if (bnklogo != null) {
            var CHUNK_SIZE = 0x8000; // arbitrary number here, not too small, not too big
            var index = 0;
            var length = bnklogo.length;
            var result = '';
            var slice;
            while (index < length) {
                slice = bnklogo.slice(index, Math.min(index + CHUNK_SIZE, length)); // `Math.min` is not really necessary here I think
                result += String.fromCharCode.apply(null, slice);
                index += CHUNK_SIZE;
            }

            return result;//return btoa(String.fromCharCode.apply(null, bnklogo));
        }

    }

    $scope.Consumerlogofile_changed = function (element) {
        $scope.$apply(function (scope) {

            var photofile = element.files[0];
            var reader = new FileReader();
            reader.onload = function (e) {
                // handle onload
                $scope.consumerlogodata = btoa(e.target.result);

            };
            reader.readAsDataURL(element.files[0]);
            //reader.readAsBinaryString(photofile);
        });
    };

    //Submit Review Comment   
    $scope.SendReviewComment = function (form) {
        if ($scope[form].$valid) {
            var Comment = $scope.Comment;
            var ReviewId = document.getElementsByName('hdnreviewId')[0].value;
            var MerchantId = document.getElementsByName('hdnMerchantId')[0].value;

            var getData = angularService.AddReviewComment(ReviewId, MerchantId, Comment);
            getData.then(function (msg) {

                if (String(msg.data).indexOf("Navigation") >= 0) {
                    //alert(msg.data);
                    window.location = msg.data;
                }
                else {
                    alert(msg.data);
                    window.location.href = '/Home/Index';
                }
            }, function (errdata) {

                if (errdata != null) {
                    if (errdata.data != null) {
                        alert('Error in Sending coupon to Mobile No');
                    }
                }

            });


        } else {
            $scope.showMsgs = true;
        }
    };

    $scope.SubmitReview = function (form) {

        var merchantid = $("#hdnMerchantId").val();
        var consumerid = $("#hdnConsumerId").val();
        var rating1 = document.getElementsByName('input-Q1')[0].value;
        var rating2 = document.getElementsByName('input-Q2')[0].value;
        var rating3 = document.getElementsByName('input-Q3')[0].value;
        var rating4 = document.getElementsByName('input-Q4')[0].value;
        var IsSharedDECWithFriends = $scope.checkboxsenddec;
        var reviewsubmit = {
            reviewid: $scope.reviewid,
            Question1Rating: rating1,
            Question2Rating: rating2,
            Question3Rating: rating3,
            Question4Rating: rating4,
            IsSharedDECWithFriends: IsSharedDECWithFriends,
            merchantId: merchantid,
            consumerId: consumerid,
            Comment: $scope.Comment
        };

        var getData = angularService.SaveConsumerReview(reviewsubmit, merchantid);
        getData.then(function (msg) {
            if ($scope.checkboxsenddec) {
                var text = angularService.getString("ReviewMessage", $rootScope.languagecode);
                alert(text);
                text = angularService.getString("ReviewRedirectMessage", $rootScope.languagecode);
                alert(text);
                location.href = "/Consumer/SendDec/" + msg.data;
            }
            else {
                var text = angularService.getString("ReviewMessage", $rootScope.languagecode);
                alert(text);
                location.href = "/Consumer/MerchantPendingReviews";
                //if (rating1 == 1 || rating2 == 1 || rating3 == 1 || rating4 == 1) {
                //    location.href = "/Consumer/MerchantPendingReviewsComment?merchant=" + merchantid + "&review=" + reviewsubmit.reviewid;
                //}
                //else {
                //    location.href = "/Consumer/MerchantPendingReviews";
                //}

            }
        }, function () {
            alert('Error in updating Consumer');
        });
    }

    $scope.showDEC = function (bank) {
        $rootScope.searchButtonText = true;
        $rootScope.divEmptyCoupons = false;
        $scope.b64encoded = 'none';
        $scope.b64banklogoencoded = 'none';
        $scope.b64decencoded = 'none';
        $scope.b64merdecencoded = 'none';
        $scope.b64merlogoencoded = 'none';
        $scope.b64conlogoencoded = 'none';

        $scope.button1_text = bank.button1_text;
        $scope.button2_text = bank.button2_text;
        $scope.button2_url = bank.button2_url;
        $scope.button3_text = bank.button3_text;
        $scope.button3_url = bank.button3_url;
        $scope.bankid = bank.bankid;
        $scope.bankname = bank.bankname;

        var getData = angularService.getBankDECDetails(bank.bankid);
        getData.then(function (bnkdec) {

            $rootScope.divDEC = true;
            $rootScope.divBanks = false;
            $rootScope.divCoupons = false;
            $rootScope.divCouponDetails = false;
            $rootScope.divMerchantDEC = false;

            //$scope.bankid = bnkdec.data.bankid;
            $scope.decid = bnkdec.data.decid;
            $scope.decname = bnkdec.data.decname;
            //$scope.b64encoded = btoa(String.fromCharCode.apply(null, bnkdec.data.decimage));
            if (bnkdec.data.decimage != null) {
                var CHUNK_SIZE = 0x8000; // arbitrary number here, not too small, not too big
                var index = 0;
                var length = bnkdec.data.decimage.length;
                var result = '';
                var slice;
                while (index < length) {
                    slice = bnkdec.data.decimage.slice(index, Math.min(index + CHUNK_SIZE, length)); // `Math.min` is not really necessary here I think
                    result += String.fromCharCode.apply(null, slice);
                    index += CHUNK_SIZE;
                }

                $scope.b64encoded = result;//String.fromCharCode.apply(null, coupon.DEC);                
            }

            if (bank.bank_logo != null) {
                var CHUNK_SIZE = 0x8000; // arbitrary number here, not too small, not too big
                var index = 0;
                var length = bank.bank_logo.length;
                var resultnew = '';
                var slice;
                while (index < length) {
                    slice = bank.bank_logo.slice(index, Math.min(index + CHUNK_SIZE, length)); // `Math.min` is not really necessary here I think
                    resultnew += String.fromCharCode.apply(null, slice);
                    index += CHUNK_SIZE;
                }

                $scope.b64banklogoencoded = resultnew;//String.fromCharCode.apply(null, coupon.DEC);
            }

            //Get onsumer logo

            var getConsumerData = angularService.getConsumerLogo();
            getConsumerData.then(function (consumerlogo) {
                if (consumerlogo.data.consumerlogo != null) {
                    var CHUNK_SIZE = 0x8000; // arbitrary number here, not too small, not too big
                    var index = 0;
                    var length = consumerlogo.data.consumerlogo.length;
                    var resultnew = '';
                    var slice;
                    while (index < length) {
                        slice = consumerlogo.data.consumerlogo.slice(index, Math.min(index + CHUNK_SIZE, length)); // `Math.min` is not really necessary here I think
                        resultnew += String.fromCharCode.apply(null, slice);
                        index += CHUNK_SIZE;
                    }

                    $scope.b64conlogoencoded = resultnew;//String.fromCharCode.apply(null, coupon.DEC);
                }
                $rootScope.searchButtonText = false;
            });


        }, function (errdata) {
            $rootScope.searchButtonText = false;
            if (errdata != null) {
                if (errdata.data != null) {
                    alert(errdata.data);
                    alert('Error in getting records showDEC');
                }
            }
        });
    }

    $scope.showMerchantDEC = function (merchant) {

        $rootScope.searchButtonText = true;
        $rootScope.divEmptyCoupons = false;
        $scope.b64encoded = 'none';
        $scope.b64banklogoencoded = 'none';
        $scope.b64decencoded = 'none';
        $scope.b64merdecencoded = 'none';
        $scope.b64merlogoencoded = 'none';
        $scope.b64conlogoencoded = 'none';
        $scope.consumergender = 'none';

        $scope.button1_text = merchant.button1_text;
        $scope.button2_text = merchant.button2_text;
        $scope.button2_url = merchant.button2_url;
        $scope.button3_text = merchant.button3_text;
        $scope.button3_url = merchant.button3_url;
        $scope.button4_text = merchant.button4_text;

        $scope.MerchantId = merchant.merchantid;
        $scope.MerchantName = merchant.MerchantName;
        $scope.Points = 0;
        $scope.RewardName = "Reward Points";

        var getData = angularService.getMerchantDECDetails(merchant.merchantid);
        getData.then(function (merchantdec) {
            //$scope.bankid = bnkdec.data.bankid;
            $scope.decname = merchantdec.data.DECName;
            if (merchantdec.data.RewardName != null) {
                if (merchantdec.data.RewardName != "") {
                    $scope.RewardName = merchantdec.data.RewardName;
                }
            }


            if (merchantdec.data.MerchantDEC != null) {
                var CHUNK_SIZE = 0x8000; // arbitrary number here, not too small, not too big
                var index = 0;
                var length = merchantdec.data.MerchantDEC.length;
                var result = '';
                var slice;
                while (index < length) {
                    slice = merchantdec.data.MerchantDEC.slice(index, Math.min(index + CHUNK_SIZE, length)); // `Math.min` is not really necessary here I think
                    result += String.fromCharCode.apply(null, slice);
                    index += CHUNK_SIZE;
                }
                $scope.b64merdecencoded = result;//String.fromCharCode.apply(null, cpn.data.DEC);
            }
            else if (merchantdec.data.merchantDecFromLibrary != null) {
                $scope.b64merdecencoded = merchantdec.data.merchantDecFromLibrary;
            }

            $scope.DECColor = merchantdec.data.DECColor;

            if (merchantdec.data.MerchantLogo != null) {
                var CHUNK_SIZE = 0x8000; // arbitrary number here, not too small, not too big
                var index = 0;
                var length = merchantdec.data.MerchantLogo.length;
                var resultnew = '';
                var slice;
                while (index < length) {
                    slice = merchantdec.data.MerchantLogo.slice(index, Math.min(index + CHUNK_SIZE, length)); // `Math.min` is not really necessary here I think
                    resultnew += String.fromCharCode.apply(null, slice);
                    index += CHUNK_SIZE;
                }

                $scope.b64merlogoencoded = resultnew;//String.fromCharCode.apply(null, cpn.data.DEC);
            }


            //Get onsumer logo
            var getConsumerData = angularService.getConsumerLogo();
            getConsumerData.then(function (consumerlogo) {

                if (consumerlogo.data.Gender != null) {
                    if (consumerlogo.data.Gender != "") {
                        $scope.consumergender = consumerlogo.data.Gender;
                    }
                }
                if (consumerlogo.data.consumerlogo != null) {
                    var CHUNK_SIZE = 0x8000; // arbitrary number here, not too small, not too big
                    var index = 0;
                    var length = consumerlogo.data.consumerlogo.length;
                    var resultnew = '';
                    var slice;
                    while (index < length) {
                        slice = consumerlogo.data.consumerlogo.slice(index, Math.min(index + CHUNK_SIZE, length)); // `Math.min` is not really necessary here I think
                        resultnew += String.fromCharCode.apply(null, slice);
                        index += CHUNK_SIZE;
                    }

                    $scope.b64conlogoencoded = resultnew;//String.fromCharCode.apply(null, cpn.data.DEC);
                }
            });

            //Get onsumer points
            var getConsumerPoints = angularService.getConsumerPoints($scope.MerchantId);
            getConsumerPoints.then(function (consumerpoints) {
                if (consumerpoints.data.Points != null) {
                    $scope.Points = consumerpoints.data.Points;

                    if (consumerpoints.data.iscashback == true) {
                        $scope.ShowCurrency = true;
                    }
                    else {
                        $scope.ShowCurrency = false;
                    }
                }
                else {
                    $scope.ShowCurrency = false;
                }
            });
            $rootScope.divDEC = false;
            $rootScope.divBanks = false;
            $rootScope.divCoupons = false;
            $rootScope.divCouponDetails = false;
            $rootScope.divMerchantDEC = true;
            $rootScope.searchButtonText = false;
        }, function (errdata) {
            $rootScope.searchButtonText = false;
            if (errdata != null) {
                if (errdata.data != null) {
                    alert(errdata.data);
                    alert('Error in getting records showMerchantDEC');
                }
            }
        });
    }

    $scope.showValidTill = function (validtill) {
        if (validtill != null) {
            validtill = validtill;
            validtill = validtill.replace('/Date(', '');
            validtill = validtill.replace(')/', '');
            return new Date(parseInt(validtill)).toDateString();
        }
    }

    $scope.showCoupon = function (coupon) {

        $rootScope.searchButtonText = true;
        $rootScope.divEmptyCoupons = false;
        $rootScope.divDEC = false;
        $rootScope.divBanks = false;
        $rootScope.divCoupons = false;
        $rootScope.divCouponDetails = true;
        $rootScope.divMerchantDEC = false;
        $scope.b64decencoded = 'none';
        $scope.coupondecdata = null;

        //$scope.b64encodedqr = 'none';
        // $scope.couponqrcodedata = null;
        //var getData = angularService.getCouponDetails(coupon.couponid);
        //getData.then(function (cpn) {

        $scope.categoryid = coupon.categoryid;
        $scope.coupontitle = coupon.CouponTitle;
        $scope.couponcode = coupon.CouponCode;
        $scope.MerchantId = coupon.MerchantId;

        $scope.EventConditions = null;


        var getEventConditions = angularService.getEventConditions(coupon.couponid);
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

        $scope.CouponConditions = null;


        var getCouponConditions = angularService.getCouponConditions(coupon.couponid);
        getCouponConditions.then(function (condition) {

            if (condition.data.length != 0) {
                $scope.CouponConditions = condition.data;
            }
        },
        function (errdata) {

            if (errdata != null) {
                if (errdata.data != null) {
                    alert(errdata.data);
                    $rootScope.searchButtonText = false;
                    alert("Error in getting Coupon condition");
                }
            }
        });

        if (coupon.ValidTill != null) {
            $scope.validtill = coupon.ValidTill;
            $scope.validtill = $scope.validtill.replace('/Date(', '');
            $scope.validtill = $scope.validtill.replace(')/', '');
            $scope.validtill = new Date(parseInt($scope.validtill)).toDateString();
        }

        if (coupon.DEC != null) {
            var CHUNK_SIZE = 0x8000; // arbitrary number here, not too small, not too big
            var index = 0;
            var length = coupon.DEC.length;
            var result = '';
            var slice;
            while (index < length) {
                slice = coupon.DEC.slice(index, Math.min(index + CHUNK_SIZE, length)); // `Math.min` is not really necessary here I think
                result += String.fromCharCode.apply(null, slice);
                index += CHUNK_SIZE;
            }


            $scope.b64decencoded = result;//String.fromCharCode.apply(null, coupon.DEC);
            $scope.coupondecdata = btoa($scope.b64decencoded);
        }

        //if (coupon.QRCode != null) {
        //    var QRCHUNK_SIZE = 0x8000; // arbitrary number here, not too small, not too big
        //    var QRindex = 0;
        //    var QRlength = coupon.QRCode.length;
        //    var QRresult = '';
        //    var QRslice;
        //    while (QRindex < QRlength) {
        //        QRslice = coupon.QRCode.slice(QRindex, Math.min(QRindex + QRCHUNK_SIZE, QRlength)); // `Math.min` is not really necessary here I think
        //        QRresult += String.fromCharCode.apply(null, QRslice);
        //        QRindex += QRCHUNK_SIZE;
        //    }


        //    $scope.b64encodedqr = btoa(QRresult);// String.fromCharCode.apply(null, coupon.QRCode);
        //    $scope.couponqrcodedata = $scope.b64encodedqr;
        //}



        $scope.coupondetails = coupon.CouponDetails;

        var getMerchantDetails = angularService.getMerchantDetails(coupon.MerchantId);
        getMerchantDetails.then(function (merchant) {
            $scope.merchant = merchant.data;
        },
        function (errdata) {
            alert(errdata.data);
            $rootScope.searchButtonText = false;
            alert("Error in getting merchant details");
        });
        //},
        //    function () {
        //        alert(errdata.data);
        //        $rootScope.searchButtonText = false;
        //        alert("Error in getting coupon details");
        //    });
        $rootScope.searchButtonText = false;
    }


    $scope.showCoupons = function (bankid) {

        //$rootScope.searchButtonText = true;


        $scope.bankid = $scope.bankid;

        fillCityList();
        fillBusinessCategoryList();

        // GetAllCoupons($scope.cityid, $scope.locationid, $scope.categoryid, $scope.bankid);

        //$q.all([one.promise, two.promise, three.promise]).then(function () {
        //    //console.log("ALL INITIAL PROMISES RESOLVED");

        //});

        $rootScope.divDEC = false;
        $rootScope.divBanks = false;
        $rootScope.divMerchantDEC = false;
        $rootScope.divCouponDetails = false;
        $rootScope.divCoupons = true;
        $rootScope.divBankCoupons = true;
        $scope.gridOptions.data = null;
        $rootScope.searchButtonText = false;

        //$rootScope.searchButtonText = false;
    }

    $scope.showMerchantCoupons = function (merchantid) {
        $rootScope.searchButtonText = true;

        GetAllMerchantCoupons($scope.MerchantId);
        // $rootScope.searchButtonText = false;
    }


    $scope.UpdateCouponsData = function (loc) {

        if (loc != null) {
            $scope.locationid = loc.LocationId;
            GetAllCoupons($scope.cityid, $scope.locationid, $scope.categoryid, $scope.bankid);
        }
        else {
            $scope.locationid = 0;
            GetAllCoupons($scope.cityid, $scope.locationid, $scope.categoryid, $scope.bankid);
        }
    }

    $scope.GetCouponsFromCity = function (cty) {

        if (cty != null) {
            $scope.cityid = cty.City;
            // GetAllCoupons(cty.City, "0", $scope.categoryid, $scope.bankid);
        }
        else {
            // $scope.locationid = 0;
            $scope.cityid = "0";
            //GetAllCoupons("0", "0", $scope.categoryid, $scope.bankid);
        }
    }

    $scope.GetCouponsFromCityCategory = function () {

        $rootScope.searchButtonText = true;

        if ($scope.cityid == 0 && $scope.categoryid == 0) {
            var text = angularService.getString("CityCategoryPrompt", $rootScope.languagecode);
            alert(text);
            $rootScope.searchButtonText = false;
        }
        else {

            GetAllCoupons($scope.cityid, "0", $scope.categoryid, $scope.bankid);
        }
    }

    $scope.UpdateCouponsDataForBusiness = function (busi) {
        if (busi != null) {
            $scope.categoryid = busi.categoryid;
            //GetAllCoupons($scope.cityid, $scope.locationid, $scope.categoryid, $scope.bankid);
        }
        else {
            $scope.categoryid = 0;
            //GetAllCoupons($scope.cityid, $scope.locationid, $scope.categoryid, $scope.bankid);
        }
    }

    $scope.CalculateTaxes = function (denom) {

        if (denom != undefined && denom != "") {
            var DiscPercent = document.getElementsByName('hdngiftcarddiscount')[0].value;
            Discount = denom * (DiscPercent / 100);
            denom = denom - Discount;
            $scope.DiscountedValue = denom;
            if ($scope.Quantity != undefined && $scope.Quantity != "") {
                var TaxesPercent = document.getElementsByName('hdngiftcardtax')[0].value;
                $scope.Amount = parseFloat(denom) * parseFloat($scope.Quantity);

                $scope.Taxes = ($scope.Amount) * parseFloat(TaxesPercent / 100);

                $scope.GrandTotal = parseInt($scope.Amount + $scope.Taxes);
            }
            else {
                $scope.Amount = 0;
                $scope.Taxes = 0;
                $scope.GrandTotal = 0;
            }
        }
        else {
            $scope.DiscountedValue = 0;
            $scope.Amount = 0;
            $scope.Taxes = 0;
            $scope.GrandTotal = 0;
        }
    }

    $scope.hideDEC = function () {
        $scope.b64encoded = 'none';
        $rootScope.divDEC = false;
        $rootScope.divBanks = true;
        $rootScope.divCoupons = false;
        $rootScope.divMerchantDEC = false;
        $rootScope.divCouponDetails = false;
        $scope.locationid = 0;
        $scope.cityid = 0;
        $scope.categoryid = 0;
        $scope.bankid = 0;
    }

    $scope.NavigateBack = function () {
        if ($rootScope.divCouponDetails == true) {
            //Go back to coupons
            $rootScope.divDEC = false;
            $rootScope.divBanks = false;
            $rootScope.divCoupons = true;
            $rootScope.divMerchantDEC = false;
            $rootScope.divCouponDetails = false;
        }
        else if ($rootScope.divCoupons == true) {
            //Go back to DEC
            if ($rootScope.divBankCoupons == true) {
                $rootScope.divDEC = true;
                $rootScope.divMerchantDEC = false;
            }
            else {
                $rootScope.divDEC = false;
                $rootScope.divMerchantDEC = true;
            }

            $rootScope.divBanks = false;
            $rootScope.divCoupons = false;
            $rootScope.divCouponDetails = false;

        }
        else if ($rootScope.divMerchantDEC == true) {
            //Go back to bank list
            $rootScope.divDEC = false;
            $rootScope.divBanks = true;
            $rootScope.divCoupons = false;
            $rootScope.divCouponDetails = false;
            $rootScope.divMerchantDEC = false;
        }
        else if ($rootScope.divDEC == true) {
            //Go back to bank list
            $rootScope.divDEC = false;
            $rootScope.divBanks = true;
            $rootScope.divCoupons = false;
            $rootScope.divCouponDetails = false;
            $rootScope.divMerchantDEC = false;
        }
        else if ($rootScope.divBanks == true) {
            //Go back to home page
            window.history.back();
        }
        else {
            window.history.back();
        }
    }

    $scope.redirect = function () {
        if ($scope.button2_url != null) {
            if ($scope.button2_url != undefined) {
                if ($scope.button2_url != "") {
                    window.location.href = $scope.button2_url;
                }
                else {
                    if ($scope.button2_text != "Benefits") {
                        window.location.href = "/Consumer/Redeem";
                    }
                    else {
                        window.location.href = "/Consumer/Benefits?MerchantId=" + $scope.MerchantId;
                    }
                }
            }
            else {
                if ($scope.button2_text != "Benefits") {
                    window.location.href = "/Consumer/Redeem";
                }
                else {
                    window.location.href = "/Consumer/Benefits?MerchantId=" + $scope.MerchantId;
                }
            }
        }
        else {
            if ($scope.button2_text != "Benefits") {
                window.location.href = "/Consumer/Redeem";
            }
            else {
                window.location.href = "/Consumer/Benefits?MerchantId=" + $scope.MerchantId;

            }
        }
    }


    $scope.sendGC = function (Id) {

        var merchantid = $("#hdnMerchantId").val();
        window.location.href = "/Consumer/SendGiftCard?Id=" + Id + "&MerchantId=" + merchantid;
    }

    $scope.redirect1 = function (merchantid) {

        if ($scope.button3_url != null) {
            if ($scope.button3_url != undefined) {
                if ($scope.button3_url != "") {
                    window.location.href = $scope.button3_url;
                }
                else {
                    var consumerid = $("#hdnConsumerId").val();
                    window.location.href = "https://www.offertraker.com/order/select.php?MerchantId=" + merchantid + "&UserId=" + consumerid;
                }
            }
            else {
                var consumerid = $("#hdnConsumerId").val();
                window.location.href = "https://www.offertraker.com/order/select.php?MerchantId=" + merchantid + "&UserId=" + consumerid;
            }
        }
        else {
            var consumerid = $("#hdnConsumerId").val();
            window.location.href = "https://www.offertraker.com/order/select.php?MerchantId=" + merchantid + "&UserId=" + consumerid;
        }
    }

    $scope.showMerchantOrders = function (merchantid) {

        $rootScope.searchButtonText = true;
        var consumerid = $("#hdnConsumerId").val();
        window.location.href = "https://www.offertraker.com/order/select.php?MerchantId=" + merchantid + "&UserId=" + consumerid;
    }

    $scope.AddUpdateConsumer = function (form) {

        var consumerid = document.getElementsByName('hdnConsumerId')[0].value;
        if ($scope[form].$valid) {

            var ccode = $scope.selectedcountrycode2object;
            var ccode1 = $scope.selectedcountrycode3object;

            var consumer = {
                id: $scope.id,
                consumername: $scope.Name,
                Gender: $scope.gender,
                consumerlogo: $scope.consumerlogodata,
                BuildingName: $scope.BuildingName,
                SocietyName: $scope.SocietyName,
                Country: $scope.countryid,
                State: $scope.stateid,
                Street: $scope.Street,
                Location: $scope.Location,
                City: $scope.City,
                PinCode: $scope.PinCode,
                DOB: $scope.DOB,
                DOA: $scope.DOA,
                Email: $scope.Email,
                Phone1: $scope.Phone1,
                Phone2: $scope.Phone2,
                Phone3: $scope.Phone3,
                UserId: $scope.UserId,
                DOB: $scope.DOB,
                DOA: $scope.DOA
            };

            if ($scope.Phone2 != undefined) {
                if ($scope.Phone2 != "") {
                    consumer.Phone2 = ccode.CountryCode + " " + $scope.Phone2;
                }
            }

            if ($scope.Phone3 != undefined) {
                if ($scope.Phone3 != "") {
                    consumer.Phone3 = ccode1.CountryCode + " " + $scope.Phone3;
                }
            }

            var getData = angularService.updateConsumer(consumer);
            getData.then(function (msg) {
                alert(msg.data);
                window.location.href = '/Home/Index';
            }, function (errdata) {
                if (errdata != null) {
                    if (errdata.data != null) {
                        alert(errdata.data);
                        alert('Error in updating Consumer');
                    }
                }

            });
        } else {
            $scope.showMsgs = true;
        }
    }


    //Send Dec to Friends
    $scope.SendDECtoConsumer = function (form) {
        if ($scope[form].$valid) {
            if ($scope.MobileNo == undefined && $scope.Email == undefined) {
                var text = angularService.getString("EmailMobileNoPrompt", $rootScope.languagecode);
                alert(text);
            }
            else if ($scope.MobileNo == "" && $scope.Email == "") {
                var text = angularService.getString("EmailMobileNoPrompt", $rootScope.languagecode);
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

                var getData = angularService.AddCheckDECConsumer(MobileNo);
                getData.then(function (msg) {

                    if (String(msg.data).indexOf("Navigation") >= 0) {
                        //alert(msg.data);
                        window.location = msg.data;
                    }
                    else {
                        alert(msg.data);
                        window.location.href = '/Home/Index';
                    }
                }, function () {
                    alert('Error in Sending DEC to Mobile No');
                });
            }

        } else {
            $scope.showMsgs = true;
        }
    };

    //Send Dec to Friends
    $scope.GetDECFromMerchant = function (form) {
        if ($scope[form].$valid) {

            if ($scope.MobileNo == undefined && document.getElementsByName('hdnConsumerPhone')[0].value == "") {
                //var text = angularService.getString("MobileNoPrompt", $rootScope.languagecode);
                alert("Please enter Mobile Number");
            }
            else if ($scope.MobileNo == "" && document.getElementsByName('hdnConsumerPhone')[0].value == "") {
                //var text = angularService.getString("MobileNoPrompt", $rootScope.languagecode);
                alert("Please enter Mobile Number");
            }
            else if ($scope.BusinessMobileNo == undefined) {
                //var text = angularService.getString("BusinessMobileNoPrompt", $rootScope.languagecode);
                alert("Please enter Business Mobile Number");
            }
            else if ($scope.BusinessMobileNo == "") {
                //var text = angularService.getString("BusinessMobileNoPrompt", $rootScope.languagecode);
                alert("Please enter Business Mobile Number");
            }
            else {
                var IsExistingConsumer = false;

                if ($scope.MobileNo == undefined) {
                    $scope.MobileNo = document.getElementsByName('hdnConsumerPhone')[0].value;
                    IsExistingConsumer = true;
                }
                else if ($scope.MobileNo == "") {
                    $scope.MobileNo = document.getElementsByName('hdnConsumerPhone')[0].value;
                    IsExistingConsumer = true;
                }


                var MobileNo = $scope.MobileNo
                var businessno = $scope.BusinessMobileNo

                if (document.getElementsByName('hdnConsumerPhone')[0].value == "") {
                    var ccode = $scope.selectedcountrycodeobject;
                    MobileNo = ccode.CountryCode + " " + MobileNo;
                }

                var mcode = $scope.selectedMerchantcountrycodeobject;
                businessno = mcode.CountryCode + " " + businessno;

                var getData = angularService.GetMerchantDEC(MobileNo, businessno);
                getData.then(function (msg) {

                    // var text = angularService.getString("ShareDECMessage", $rootScope.languagecode);

                    alert(msg.data);

                    //if (text == msg.data) {
                    if (IsExistingConsumer) {
                        window.location.href = '/Consumer/Index';
                    }
                    else {
                        window.location.href = '/Consumer/NewLogin?username=' + MobileNo;
                    }
                    //}
                    //else
                    //{
                    //    window.location.href = '/Account/Login';
                    //}

                }, function (err) {
                    if (err != null) {
                        if (err.data != null) {
                            alert('Error in getting DEC from business');
                        }
                    }
                });
            }

        } else {
            $scope.showMsgs = true;
        }
    };

    //Send Dec to Friends
    $scope.SendCoupontoConsumer = function (form) {

        if ($scope[form].$valid) {
            if ($scope.MobileNo == undefined && $scope.Email == undefined) {
                var text = angularService.getString("EmailMobileNoPrompt", $rootScope.languagecode);
                alert(text);
            }
            else if ($scope.MobileNo == "" && $scope.Email == "") {
                var text = angularService.getString("EmailMobileNoPrompt", $rootScope.languagecode);
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
                var MerchantId = document.getElementsByName('hdnMerchantId')[0].value;
                var Id = document.getElementsByName('hdnId')[0].value;

                var getData = angularService.AddCheckCouponConsumer(MobileNo, MerchantId, CouponId, Id);
                getData.then(function (msg) {

                    if (String(msg.data).indexOf("Navigation") >= 0) {
                        //alert(msg.data);
                        window.location = msg.data;
                    }
                    else {
                        alert(msg.data);
                        window.location.href = '/Home/Index';
                    }
                }, function (errdata) {

                    if (errdata != null) {
                        if (errdata.data != null) {
                            alert('Error in Sending coupon to Mobile No');
                        }
                    }

                });
            }

        } else {
            $scope.showMsgs = true;
        }
    };


    //Fill country, state, city, location
    /***********************************************************************************************************************/
    function fillCityList() {
        //$rootScope.searchButtonText = true;
        var getData = angularService.getCities();
        getData.then(function (cty) {
            $scope.CityList = cty.data;
            //$rootScope.searchButtonText = false;
        }, function (errdata) {
            $rootScope.searchButtonText = false;
            if (errdata != null) {
                if (errdata.data != null) {
                    alert(errdata.data);
                    alert('Error in getting records fillCityList');
                }
            }
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
                    alert('Error in getting records fillCountry');
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
            /* var getData = angularService.getCities(state.stateid);
             getData.then(function (city) {
                 $scope.CityList = city.data;
             }, function (errdata) {
                 if (errdata != null) {
                     if (errdata.data != null) {
                         alert(errdata.data);
                         alert('Error in getting records getCities');
                     }
                 }
             });*/
        }
        else {
            $scope.stateid = 0;
            //$scope.locationid = 0;
            //$scope.cityid = 0;
            //  $scope.LocationList = null;
            $scope.StateList = null;
            // $scope.CityList = null;
        }
    }

    $scope.getLocations = function (city) {

        if (city != null) {
            $scope.cityid = city.cityid;
            GetAllCoupons($scope.cityid, $scope.locationid, $scope.categoryid, $scope.bankid);
            var getData = angularService.getLocations(city.cityid);
            getData.then(function (loc) {
                $scope.LocationList = loc.data;
            }, function (errdata) {
                if (errdata != null) {
                    if (errdata.data != null) {
                        alert(errdata.data);
                        alert('Error in getting records getLocations');
                    }
                }

            });
        }
        else {
            $scope.locationid = 0;
            $scope.cityid = 0;
            $scope.LocationList = null;
            GetAllCoupons($scope.cityid, $scope.locationid, $scope.categoryid, $scope.bankid);
        }
    }


    $scope.LocationChanged = function (loc) {
        if (loc != null) {
            $scope.locationid = loc.LocationId;
        }
    }
    /***********************************************************************************************************************/

    //Show Merchant Gift Card
    $scope.showMerchantGiftCard = function (merchantid) {

        $scope.MerchantId = merchantid;
        location.href = "/Consumer/MerchantGiftCardsIndex/" + merchantid;
    }

    $scope.OpenMerchantGiftCardForm = function () {
        $rootScope.divBuyGiftCards = true;
        GetCountryCode();
        GetgiftcardDenimination();
    }

    //Get Denomination for merchant
    function GetgiftcardDenimination() {

        var merchantid = $("#hdnMerchantId").val();
        var getData = angularService.getGiftDenomination();
        getData.then(function (denom) {

            if (denom.data.length > 0) {
                $rootScope.divBuyGiftCards = true;
                $scope.denomination = denom.data[0];
                $scope.giftcardid = denom.data[0].GiftCardId;
                $scope.Denomination1 = denom.data[0].Denomination1;
                if ($scope.Denomination1 > 0)
                    $scope.divdenomination1 = true;

                $scope.Denomination2 = denom.data[0].Denomination2;
                if ($scope.Denomination2 > 0)
                    $scope.divdenomination2 = true;

                $scope.Denomination3 = denom.data[0].Denomination3;
                if ($scope.Denomination3 > 0)
                    $scope.divdenomination3 = true;

                $scope.Denomination4 = denom.data[0].Denomination4;
                if ($scope.Denomination4 > 0)
                    $scope.divdenomination4 = true;
            }
            else {
                $rootScope.divBuyGiftCards = false;
            }
            $scope.merchantid = merchantid;
        }, function (err) {
            if (err != null) {
                if (err.data != null) {
                    alert(err.data);
                    alert('Error in getting records GetPendingReview');
                }
            }

        });
    }

    //Pay Gift Card
    $scope.PayGiftCard = function (form) {

        var merchantid = $("#hdnMerchantId").val();
        var giftCardId = $scope.giftcardid;

        var qtyDenom1 = $scope.selecteddenominationobject;
        var qty = $scope.Quantity;
        var grandtotal = $scope.GrandTotal;
        if (grandtotal == 0) {
            alert("Please select denomination and enter quantity of gift cards.");
        }
        else {
            ////var giftCard = {
            ////    giftcardid: giftCardId,
            ////    Denomination: qtyDenom1,
            ////    Quantity: qty,
            ////    GrandTotal: grandtotal,
            ////    merchantId: merchantid
            ////};
            //
            // angularService.PayConsumerGiftCard(merchantid, giftCardId, qtyDenom1, qty, grandtotal);

            //getData.then(function (msg) {
            //    $scope.divSelectGiftCard = false;
            //    $scope.divPayForGiftCard = true;
            //    $scope.formPostUrl = msg.data.formPostUrl;
            //}, function () {
            //    alert('Error in updating Consumer');
            //});

            location.href = "/Consumer/PayForGiftCards?merchantid=" + merchantid + "&giftCardId=" + giftCardId + "&qtyDenom1=" + qtyDenom1 + "&qty=" + qty + "&GrandTotal=" + grandtotal;
            //var getData = angularService.PayConsumerGiftCard(giftCard);
            //getData.then(function (msg) {
            //    
            //    $scope.divSelectGiftCard = false;
            //    $scope.divPayForGiftCard = true;
            //    $scope.formPostUrl = msg.data.formPostUrl;
            //}, function () {
            //    alert('Error in updating Consumer');
            //});
        }
        //var giftCard = {
        //    giftcardid: $scope.giftcardid,
        //    Denomination1: $scope.Denomination1,
        //    Denomination2: $scope.Denomination2,
        //    Denomination3: $scope.Denomination3,
        //    Denomination4: $scope.Denomination4,
        //    qtyDenom1: $scope.qtyDenom1,
        //    qtyDenom2: $scope.qtyDenom2,
        //    qtyDenom3: $scope.qtyDenom3,
        //    qtyDenom4: $scope.qtyDenom4,
        //    merchantId: merchantid
        //};
        //var getData = angularService.SaveConsumerGiftCard(giftCard);
        //getData.then(function (msg) {
        //    alert(msg.data);
        //    location.href = "/Home/Index"
        //}, function () {
        //    alert('Error in updating Consumer');
        //});
    }

    //Get Denomination for merchant
    function GetConsumerGiftCards() {
        var merchantid = $("#hdnMerchantId").val();
        $scope.merchantid = merchantid;
        var getData = angularService.getConsumerGiftCards($scope.merchantid);
        getData.then(function (denom) {

            if (denom.data.length > 0) {
                $rootScope.divPurchasedCards = true;
                for (i = 0; i < denom.data.length; i++) {
                    if (denom.data[i].MerchantDEC != null) {
                        var CHUNK_SIZE = 0x8000; // arbitrary number here, not too small, not too big
                        var index = 0;
                        var length = denom.data[i].MerchantDEC.length;
                        var result = '';
                        var slice;
                        while (index < length) {
                            slice = denom.data[i].MerchantDEC.slice(index, Math.min(index + CHUNK_SIZE, length)); // `Math.min` is not really necessary here I think
                            result += String.fromCharCode.apply(null, slice);
                            index += CHUNK_SIZE;
                        }
                        denom.data[i].MerchantDEC = result;//String.fromCharCode.apply(null, cpn.data.DEC);
                    }
                    else if (denom.data[i].merchantDecFromLibrary != null) {
                        denom.data[i].MerchantDEC = denom.data[i].merchantDecFromLibrary;
                    }

                    if (denom.data[i].MerchantLogo != null) {
                        var CHUNK_SIZE = 0x8000; // arbitrary number here, not too small, not too big
                        var index = 0;
                        var length = denom.data[i].MerchantLogo.length;
                        var resultnew = '';
                        var slice;
                        while (index < length) {
                            slice = denom.data[i].MerchantLogo.slice(index, Math.min(index + CHUNK_SIZE, length)); // `Math.min` is not really necessary here I think
                            resultnew += String.fromCharCode.apply(null, slice);
                            index += CHUNK_SIZE;
                        }

                        denom.data[i].MerchantLogo = resultnew;//String.fromCharCode.apply(null, cpn.data.DEC);
                    }

                    if (denom.data[i].ConsumerLogo != null) {
                        var CHUNK_SIZE = 0x8000; // arbitrary number here, not too small, not too big
                        var index = 0;
                        var length = denom.data[i].ConsumerLogo.length;
                        var resultlogo = '';
                        var slice;
                        while (index < length) {
                            slice = denom.data[i].ConsumerLogo.slice(index, Math.min(index + CHUNK_SIZE, length)); // `Math.min` is not really necessary here I think
                            resultlogo += String.fromCharCode.apply(null, slice);
                            index += CHUNK_SIZE;
                        }
                        denom.data[i].ConsumerLogo = resultlogo;
                    }

                    if (denom.data[i].ValidTill != null) {
                        $scope.validtill = denom.data[i].ValidTill;
                        $scope.validtill = $scope.validtill.replace('/Date(', '');
                        $scope.validtill = $scope.validtill.replace(')/', '');
                        $scope.validtill = new Date(parseInt($scope.validtill)).toDateString();
                        denom.data[i].ValidTill = $scope.validtill;
                    }

                }
                $scope.cards = denom.data;
            }
            else {
                $rootScope.divPurchasedCards = false;
            }
        }, function (err) {
            if (err != null) {
                if (err.data != null) {
                    alert(err.data);
                    alert('Error in getting records GetConsumerGiftCards');
                }
            }

        });
    }

    //Send Dec to Friends
    $scope.SendGCtoConsumer = function (form) {

        if ($scope[form].$valid) {

            if ($scope.MobileNo == undefined && $scope.Email == undefined) {
                var text = angularService.getString("EmailMobileNoPrompt", $rootScope.languagecode);
                alert(text);
            }
            else if ($scope.MobileNo == "" && $scope.Email == "") {
                var text = angularService.getString("EmailMobileNoPrompt", $rootScope.languagecode);
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
                var Id = $("#hdnCardId").val();
                var merchantid = $("#hdnMerchantId").val();

                var getData = angularService.SendGCConsumer(MobileNo, Id, merchantid);
                getData.then(function (msg) {
                    alert(msg.data);
                    window.location.href = '/Home/Index';
                }, function () {
                    alert('Error in Sending GC to Mobile No');
                });
            }

        } else {
            $scope.showMsgs = true;
        }
    };
    /***********************************************************************************************************************/
});

