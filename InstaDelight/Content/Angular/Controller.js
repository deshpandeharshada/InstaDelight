
app.controller("myCntrl", function ($scope, $rootScope, angularService) {
    $rootScope.searchButtonText = false;

    $scope.divMerchants = true;
    $scope.divMerchant = false;

    $rootScope.showFooter = true;

    $scope.divBanks = true;
    $scope.divBank = false;

    $rootScope.divSearch = true;
    $rootScope.divEditMerchant = false;

    $rootScope.divSupport = false;
    $rootScope.divAdminIndex = true;
    $rootScope.divVAR = false;
    $rootScope.divSettings = false;
    $rootScope.divSetBankBenefits = false;
    $scope.selectAll = false;

    $scope.data = 'none';
    $scope.logodata = 'none';
    $scope.b64encoded = 'none';
    $scope.b64decencoded = 'none';

    $scope.isCheckboxSelected = function (index) {
        return index === $scope.checkboxSelection;
    };


    $scope.merchantlogodata = null;
    $scope.merchantdecdata = null;
    /* For merchant configuration */
    $scope.CountryList = null;
    $scope.StateList = null;
    $scope.CategoryList = null;
    $scope.LocationList = null;
    $scope.CityList = null;

    $scope.selectedMerchantId = 0;

    $scope.locationid = 0;
    $scope.cityid = 0;
    $scope.categoryid = 0;
    $scope.countryid = 0;
    $scope.stateid = 0;

    $scope.selectedCategory = "";

    $scope.lblCountryCode = "";

    $scope.numRows = 5;
    $scope.gridVAROptions = {
        columnDefs: [
       { name: 'UserName', headerCellTemplate: '<div></div>', cellClass: 'htabGrid', resizable: false, cellTemplate: '<div style="cursor:pointer" ng-click="grid.appScope.editVAR(row.entity)">{{row.entity.UserName}}</div>' },
        { name: 'VARCode', cellClass: 'htabGridshare', cellTemplate: ' <div style="cursor:pointer" ng-click="grid.appScope.editVAR(row.entity)"> {{row.entity.VARCode}}  </div>', headerCellTemplate: '<div></div>', resizable: false }
        ],
        enableColumnResize: false,
        rowHeight: 65,
        showHeader: false,
        paginationPageSize: $scope.numRows,
        paginationPageSizes: [5, 10, 50, 100],
        enablePaginationControls: true,
        paginationCurrentPage: 1,
    };

    $scope.gridVAROptions.onRegisterApi = function (gridApi) {
        $scope.gridApi2 = gridApi;
    }


    $scope.showValidTill = function (validtill) {

        if (validtill != null) {
            validtill = validtill;
            validtill = validtill.replace('/Date(', '');
            validtill = validtill.replace(')/', '');
            return new Date(parseInt(validtill)).toDateString();
        }
    }

    $scope.ShowSupportDiv = function () {
        $rootScope.showFooter = true;
        $rootScope.divSupport = true;
        $rootScope.divAdminIndex = false;
        $rootScope.divSettings = false;
        $rootScope.divVAR = false;
    }

    $scope.ShowVARDiv = function () {
        $rootScope.showFooter = true;
        $rootScope.divSupport = false;
        $rootScope.divAdminIndex = false;
        $rootScope.divSettings = false;
        $rootScope.divVAR = true;
    }

    $scope.ShowBusinessSettingsDiv = function () {
        $rootScope.showFooter = true;
        $rootScope.divSupport = false;
        $rootScope.divAdminIndex = false;
        $rootScope.divVAR = false;
        $rootScope.divSettings = true;

    }

    $scope.openCreateVAR = function () {

        $scope.divCreateVAR = true;
        var countryId = document.getElementsByName('hdnCountry')[0].value;
        if (countryId != undefined) {
            if (countryId != "0") {
                {
                    fillCountry();

                    var getCountryData = angularService.getCountrycode(countryId);
                    getCountryData.then(function (mch) {

                        $scope.selectedcountrycodeobject = { countryid: countryId, CountryCode: mch.data };
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

        fillVARList();
    }


    function fillVARList() {
        var getData = angularService.getVARCodes();
        getData.then(function (varlist) {

            $scope.VARCodeList = varlist.data;
        }, function (caterr) {
            if (caterr.data != null) {
                alert('Error in getting records fillVARList');
            }
        });
    }

    $scope.OpenEditVARForm = function () {
        $scope.divVar = false;
        $scope.divVarList = true;

        $scope.loaderMore = true;
        $scope.lblMessage = 'loading please wait....!';
        $scope.result = "color-green";

        var getData = angularService.getVARUsers();

        getData.then(function (usr) {

            $scope.gridVAROptions.excessRows = usr.data.length;
            $scope.gridVAROptions.data = usr.data;
            $scope.gridVAROptions.totalItems = usr.data.length;
            if (usr.data.length < $scope.numRows) {
                $scope.gridVAROptions.minRowsToShow = usr.data.length;
            }
            else {
                $scope.gridVAROptions.minRowsToShow = $scope.numRows;
            }

        }, function (mcherror) {
            if (mcherror.data != null) {
                alert(mcherror.data);
                alert('Error in getting records OpenEditVARForm');
            }
        });
    }

    $scope.OpenEditUserForm = function () {
        $scope.divVar = false;
        $scope.divVarList = true;

        $scope.loaderMore = true;
        $scope.lblMessage = 'loading please wait....!';
        $scope.result = "color-green";

        var getData = angularService.getSupportUsers();

        getData.then(function (usr) {

            $scope.gridVAROptions.excessRows = usr.data.length;
            $scope.gridVAROptions.data = usr.data;
            $scope.gridVAROptions.totalItems = usr.data.length;
            if (usr.data.length < $scope.numRows) {
                $scope.gridVAROptions.minRowsToShow = usr.data.length;
            }
            else {
                $scope.gridVAROptions.minRowsToShow = $scope.numRows;
            }

        }, function (mcherror) {
            if (mcherror.data != null) {
                alert(mcherror.data);
                alert('Error in getting records OpenEditUserForm');
            }
        });
    }

    $scope.editVAR = function (VAR) {

        var countryId = 0;

        if (VAR.VARCode == "INSTADELIGHT") {
            countryId = 1;
        }
        else if (VAR.VARCode.match("IND")) {
            countryId = 1;
        }
        else if (VAR.VARCode.match("UAE")) {
            countryId = 3;
        }
        else if (VAR.VARCode.match("USA")) {
            countryId = 2;
        }

        else if (VAR.VARCode.match("UK")) {
            countryId = 4;
        }
        else {
            countryId = 1;
        }

        $scope.divVar = true;
        $scope.divVarList = false;


        fillCountry();


        $scope.Id = VAR.Id;
        $scope.VARName = VAR.OwnerName;
        $scope.OwnerName = VAR.FirstName;
        $scope.PhoneNumber = VAR.PhoneNumber;

        if ($scope.PhoneNumber != null) {
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
                            alert('Error in getting records editconsumer/getcountryfromcode');
                        }
                    }

                });
            }
            else {
                var getCountryData = angularService.getCountrycode(countryId);
                getCountryData.then(function (mch) {
                    $scope.selectedcountrycodeobject = { countryid: countryId, CountryCode: mch.data };
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


        $scope.Email = VAR.Email;
        $scope.VARCode = VAR.VARCode;


        if (VAR.UserName == VAR.Email) {
            $scope.PrimaryVARIDSelection = "email";
        }
        else {
            $scope.PrimaryVARIDSelection = "cell";
        }
    }

    $scope.OpenMerchantForm = function () {
        $rootScope.showFooter = true;
        fillCountry();
        getCurrency();

        GetAllMerchants();

        fillBusinessCategoryList();
        // fillCityList();     
    }

    $scope.OpenBankForm = function () {
        $rootScope.showFooter = true;
        GetAllBanks();
    }

    $scope.OpenHomeScreen = function () {
        $rootScope.showFooter = false;
        $rootScope.divAdminIndex = true;
        $rootScope.divSupport = false;
        $rootScope.divVAR = false;
    }
    $scope.EnableHomeBackButton = function () {
        $rootScope.showFooter = true;
    }

    $scope.OpenSeachCriteria = function () {
        fillCountry();
        fillVARList();
    }

    function fillBusinessCategoryList() {
        var getData = angularService.getBusinessCategory();
        getData.then(function (busi) {
            $scope.CategoryList = busi.data;
        }, function (errdata) {
            alert(errdata.data);
            alert('Error in getting records fillBusinessCategoryList');
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

    function GetCountryCode() {

        var getData = angularService.getCountrycode();
        getData.then(function (cty) {
            // $scope.countrycode = cty.data;
            $scope.lblCountryCode = cty.data;
        }, function () {
            alert('Error in getting records');
        });
    }

    $scope.getCities = function (state) {
        if (state != null) {
            $scope.stateid = state.stateid;
        }
        else {
            $scope.stateid = 0;

            $scope.StateList = null;

        }
    }


    function fillCityList() {
        var getData = angularService.getCities();
        getData.then(function (cty) {
            $scope.CityList = cty.data;
        }, function () {
            alert('Error in getting records fillCityList');
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

    $scope.getLocations = function (city) {
        if (city != null) {
            $scope.cityid = city.cityid;
            var getData = angularService.getLocations(city.cityid);
            getData.then(function (loc) {
                $scope.LocationList = loc.data;
            }, function (locerr) {
                alert('Error in getting records getLocations' + locerr.data);
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

    //From sarvatra admin
    /*********************************************************-*/
    $scope.ShowImageGallery = function () {
        $rootScope.divSearch = false;
        $rootScope.divEditMerchant = false;
        $rootScope.divImageGallery = true;
        $rootScope.divEnlargeImage = false;
    }

    $scope.CloseImageGallery = function () {
        $rootScope.divEditMerchant = true;
        $rootScope.divSearch = false;
        $rootScope.divImageGallery = false;
        $rootScope.divEnlargeImage = false;
    }

    $scope.EnlargeImage = function (src) {
        $scope.SelectedImageSrc = src;
        $rootScope.divSearch = false;
        $rootScope.divEditMerchant = false;
        $rootScope.divImageGallery = false;
        $rootScope.divEnlargeImage = true;
    }

    $scope.CloseEnlargedImage = function () {
        $scope.SelectedImageSrc = null;
        $rootScope.divSearch = false;
        $rootScope.divEditMerchant = false;
        $rootScope.divImageGallery = true;
        $rootScope.divEnlargeImage = false;
    }

    $scope.SetImage = function (src) {
        $rootScope.divSearch = false;
        $rootScope.divEditMerchant = true;
        $scope.merchantdecdata = null;
        $rootScope.divImageGallery = false;
        $rootScope.divEnlargeImage = false;

        $scope.SelectedImageSrc = src;

    }

    $scope.VARCodeChanged = function (varcode) {

        if (varcode != null) {
            $scope.VARCode = varcode.VARCode1;
        }
    }

    $scope.AddNewVAR = function () {

        if ($scope.VARCode == undefined) {
            alert("Please enter VAR Code.");
        }
        else if ($scope.VARName == undefined) {
            alert("Please enter VAR Name");
        }
        else if ($scope.OwnerName == undefined) {
            alert("Please enter owner/manager name");
        }
        else if ($scope.PhoneNumber == undefined) {
            alert("Please enter phone number");
        }
        else if ($scope.PhoneNumber == "") {
            alert("Please enter phone number");
        }
        else if ($scope.Email == undefined) {
            alert("Please enter Email");
        }
        else if ($scope.Email == "") {
            alert("Please enter Email");
        }
        else if ($scope.PrimaryVARIDSelection == undefined) {
            alert("Please select primary Id login");
        }
        else {

            var MobileNo = "";
            var ccode = $scope.selectedcountrycodeobject;

            if ($scope.PhoneNumber != undefined) {
                if ($scope.PhoneNumber != "") {
                    MobileNo = ccode.CountryCode + " " + $scope.PhoneNumber;
                }
            }


            var merchant = {
                MerchantName: $scope.OwnerName,
                DECName: $scope.VARName,
                Email: $scope.Email,
                PhoneNumber: MobileNo,
                VARCode: $scope.VARCode,
                PrimeryIDLogin: $scope.PrimaryVARIDSelection
            };

            if ($scope.Id == undefined) {
                $scope.Id = "";
            }
            if ($scope.Id != "") {
                merchant.UserId = $scope.Id;
                var getData = angularService.EditVAR(merchant);
                getData.then(function (msg) {
                    alert(msg.data);

                    var getData = angularService.getVARUsers();

                    getData.then(function (usr) {
                        $scope.gridVAROptions.excessRows = usr.data.length;
                        $scope.gridVAROptions.data = usr.data;
                        $scope.gridVAROptions.totalItems = usr.data.length;
                        if (usr.data.length < $scope.numRows) {
                            $scope.gridVAROptions.minRowsToShow = usr.data.length;
                        }
                        else {
                            $scope.gridVAROptions.minRowsToShow = $scope.numRows;
                        }

                    }, function (mcherror) {
                        if (mcherror.data != null) {
                            alert(mcherror.data);
                            alert('Error in getting records OpenEditVARForm');
                        }
                    });


                    $scope.divVar = false;
                    $scope.divVarList = true;

                }, function () {
                    alert('Error in creating VAR');
                });
            }
            else {
                var getData = angularService.CreateVAR(merchant);
                getData.then(function (msg) {
                    alert(msg.data);

                }, function () {
                    alert('Error in creating VAR');
                });
            }
        }
    }

    $scope.AddNewVARStaff = function () {
        if ($scope.OwnerName == undefined) {
            alert("Please enter support user name");
        }
        else if ($scope.PhoneNumber == undefined) {
            alert("Please enter phone number");
        }
        else if ($scope.Email == undefined) {
            alert("Please enter Email");
        }
        else if ($scope.PrimaryVARIDSelection == undefined) {
            alert("Please select primary Id login");
        }
        else {
            if ($scope.Id == undefined) {
                $scope.Id = "";
            }

            var MobileNo = "";
            var ccode = $scope.selectedcountrycodeobject;

            if ($scope.PhoneNumber != undefined) {
                if ($scope.PhoneNumber != "") {
                    MobileNo = ccode.CountryCode + " " + $scope.PhoneNumber;
                }
            }


            var merchant = {
                MerchantName: $scope.OwnerName,
                DECName: $scope.VARName,
                Email: $scope.Email,
                PhoneNumber: MobileNo,
                PrimeryIDLogin: $scope.PrimaryVARIDSelection
            };

            if ($scope.Id != "") {
                merchant.UserId = $scope.Id;
                var getData = angularService.EditVARStaff(merchant);
                getData.then(function (msg) {
                    alert(msg.data);

                    var getData = angularService.getSupportUsers();

                    getData.then(function (usr) {
                        $scope.gridVAROptions.excessRows = usr.data.length;
                        $scope.gridVAROptions.data = usr.data;
                        $scope.gridVAROptions.totalItems = usr.data.length;
                        if (usr.data.length < $scope.numRows) {
                            $scope.gridVAROptions.minRowsToShow = usr.data.length;
                        }
                        else {
                            $scope.gridVAROptions.minRowsToShow = $scope.numRows;
                        }

                    }, function (mcherror) {
                        if (mcherror.data != null) {
                            alert(mcherror.data);
                            alert('Error in getting records AddNewVARStaff');
                        }
                    });


                    $scope.divVar = false;
                    $scope.divVarList = true;

                }, function () {
                    alert('Error in updating user');
                });
            }
            else {
                var getData = angularService.CreateVARStaff(merchant);
                getData.then(function (msg) {
                    alert(msg.data);

                }, function () {
                    alert('Error in creating VAR Staff');
                });
            }
        }
    }



    $scope.activateSelectedVAR = function () {

        if ($scope.Id != "") {
            var getData = angularService.activateVAR($scope.Id);
            getData.then(function (mch) {
                alert(mch.data);
                window.location.href = '/Sarvatra/Index';
            }, function () {
                alert('Error in getting records activateSelectedVAR');
            });
        }
    }

    $scope.deactivateSelectedVAR = function () {

        if ($scope.Id != "") {
            var getData = angularService.deactivateSelectedVAR($scope.Id);
            getData.then(function (mch) {
                alert(mch.data);
                window.location.href = '/Sarvatra/Index';
            }, function () {
                alert('Error in getting records activateSelectedVAR');
            });
        }
    }

    $scope.AddUpdateMerchant = function (form) {

        if ($scope[form].$valid) {

            var merchant = {
                DECName: $scope.MerchantName,
                button1_text: $scope.button1_text,
                button2_text: $scope.button2_text,
                button2_url: $scope.button2_url,
                button3_text: $scope.button3_text,
                button3_url: $scope.button3_url,
                button4_text: $scope.button4_text,
                BuildingName: $scope.BuildingName,
                SocietyName: $scope.SocietyName,
                Street: $scope.Street,
                Country: $scope.countryid,
                State: $scope.stateid,
                Location: $scope.Location,
                City: $scope.City,
                PinCode: $scope.PinCode,
                Email: $scope.Email,
                Category: $scope.categoryid,
                PhoneNumber: $scope.PhoneNumber,
                MerchantLogo: $scope.merchantlogodata,
                MerchantDEC: $scope.merchantdecdata,
                merchantDecFromLibrary: $scope.SelectedImageSrc,
                RewardRs: $scope.RewardRs,
                RewardPoints: $scope.RewardPts,
                RedeemRs: $scope.RedeemRs,
                RedeemPt: $scope.RedeemPt,
                JoiningBonus: $scope.JoiningBonus
            };
            if ($scope.checkboxSelection == "no") {
                merchant.RunRewardProgram = false;
            }
            else {
                merchant.RunRewardProgram = true;
            }
            var redeemoptions =
                {
                    Option1: $scope.condition1,
                    Option2: $scope.condition2,
                    Option3: $scope.condition3,
                    Option4: $scope.condition4,
                    Option5: $scope.condition5,
                }

            merchant.redeemoptions = redeemoptions;

            if ($scope.usernameSelection == "phone") {
                merchant.UserName = $scope.PhoneNumber;
            }
            else {
                merchant.UserName = $scope.Email;
            }


            //var getAction = "Add";
            var getAction = $scope.Action;

            if (getAction == "Update") {
                merchant.MerchantName = $scope.MerchantName;
                merchant.DECName = $scope.DECName;
                merchant.merchantid = $scope.merchantid;
                merchant.UserId = $scope.UserId;
                merchant.PhoneNumber = $scope.PhoneNumber;

                var getData = angularService.updateMerchant(merchant);
                getData.then(function (msg) {
                    alert(msg.data);
                    ClearMerchantFields();
                }, function (err) {
                    alert(err.data);
                    alert('Error in updating merchant');
                });
            } else {
                merchant.MerchantName = $scope.MerchantName;

                var getData = angularService.AddMerchant(merchant);
                getData.then(function (msg) {

                    alert(msg.data);
                    ClearMerchantFields();
                }, function (error) {
                    if (error.data != null) {
                        alert(error.data);
                        alert('Error in adding merchant');
                    }
                });
            }
        } else {
            $scope.showMsgs = true;
        }
    };

    $scope.editMerchant = function (merchant) {
        var getData = angularService.getMerchant(merchant.merchantid);
        getData.then(function (mch) {

            $scope.merchant = mch.data;
            $scope.merchantid = mch.data.merchantid;
            $scope.UserId = mch.data.UserId;
            $scope.MerchantName = mch.data.MerchantName;
            $scope.DECName = mch.data.DECName;
            $scope.BuildingName = mch.data.BuildingName;
            $scope.SocietyName = mch.data.SocietyName;
            $scope.Street = mch.data.Street;

            $scope.button1_text = mch.data.button1_text;

            $scope.button2_text = mch.data.button2_text;
            $scope.button2_url = mch.data.button2_url;

            $scope.button3_text = mch.data.button3_text;
            $scope.button3_url = mch.data.button3_url;

            $scope.button4_text = mch.data.button4_text;


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

            $scope.JoiningBonus = mch.data.JoiningBonus;

            if (mch.data.UserName.indexOf('@') > 0) {
                $scope.usernameSelection = 'email';
            }
            else {
                $scope.usernameSelection = 'phone';
            }
            if (mch.data.RunRewardProgram != null) {
                if (mch.data.RunRewardProgram == false) {
                    $scope.checkboxSelection = 'no';
                    $scope.condition1 = mch.data.redeemoptions.Option1;
                    $scope.condition2 = mch.data.redeemoptions.Option2;
                    $scope.condition3 = mch.data.redeemoptions.Option3;
                    $scope.condition4 = mch.data.redeemoptions.Option4;
                    $scope.condition5 = mch.data.redeemoptions.Option5;
                }
                else {
                    $scope.checkboxSelection = 'yes';
                }
            }


            GetCountryCode();

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

            $scope.selectedcatobject = { categoryid: $scope.categoryid };

            $scope.selectedcountryobject = { countryid: $scope.countryid };
            $scope.selectedstateobject = { stateid: $scope.stateid };

            $scope.getStates($scope.selectedcountryobject);

            $scope.RewardRs = mch.data.RewardRs;
            $scope.RewardPts = mch.data.RewardPoints;

            $scope.RedeemPt = mch.data.RedeemPt;
            $scope.RedeemRs = mch.data.RedeemRs;

            //Get reward formula of merchant
            //var getRewardData = angularService.getMerchantRewards(mch.data.UserId);
            //getRewardData.then(function (rwd) {

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
            $scope.divMerchant = true;
            $scope.divMerchants = false;
        }, function (errdata) {
            if (errdata != null) {
                if (errdata.data != null) {
                    alert(errdata.data);
                    alert('Error in getting records editMerchant');
                }
            }

        });
    }

    $scope.editBank = function (bank) {
        var getData = angularService.getBank(bank.bankid);
        getData.then(function (bnk) {

            $scope.bank = bnk.data;
            $scope.bankid = bnk.data.bankid;
            $scope.bankname = bnk.data.bankname;
            $scope.button1_text = bnk.data.button1_text;
            $scope.button1_url = bnk.data.button1_url;
            $scope.button2_text = bnk.data.button2_text;
            $scope.button2_url = bnk.data.button2_url;
            $scope.button3_text = bnk.data.button3_text;
            $scope.button3_url = bnk.data.button3_url;
            $scope.button4_text = bnk.data.button4_text;
            $scope.button4_url = bnk.data.button4_url;

            if (bnk.data.bank_logo != null) {
                var CHUNK_SIZE = 0x8000; // arbitrary number here, not too small, not too big
                var index = 0;
                var length = bnk.data.bank_logo.length;
                var result = '';
                var slice;
                while (index < length) {
                    slice = bnk.data.bank_logo.slice(index, Math.min(index + CHUNK_SIZE, length)); // `Math.min` is not really necessary here I think
                    result += String.fromCharCode.apply(null, slice);
                    index += CHUNK_SIZE;
                }


                $scope.b64encoded = result;//String.fromCharCode.apply(null, cpn.data.DEC);
                $scope.logodata = btoa($scope.b64encoded);
            }

            var getDECData = angularService.getBankDEC(bnk.data.bankid);
            getDECData.then(function (dec) {

                $scope.DECName = dec.data.decname;
                if (dec.data.decimage != null) {
                    var CHUNK_SIZE = 0x8000; // arbitrary number here, not too small, not too big
                    var index = 0;
                    var length = dec.data.decimage.length;
                    var result = '';
                    var slice;
                    while (index < length) {
                        slice = dec.data.decimage.slice(index, Math.min(index + CHUNK_SIZE, length)); // `Math.min` is not really necessary here I think
                        result += String.fromCharCode.apply(null, slice);
                        index += CHUNK_SIZE;
                    }


                    $scope.b64decencoded = result;//String.fromCharCode.apply(null, cpn.data.DEC);
                    $scope.data = btoa($scope.b64decencoded);
                }
            }, function (errdata) {

                if (errdata != null) {
                    if (errdata.data != null) {
                        alert(errdata.data);
                        alert('Error in getting records editBank');
                    }
                }
            });
            $scope.Action = "Update";
            $scope.divBank = true;
            $scope.divBanks = false;
        }, function (errdata) {
            if (errdata != null) {
                if (errdata.data != null) {
                    alert(errdata.data);
                    alert('Error in getting records editBank');
                }
            }

        });
    }

    $scope.editMerchantProfile = function (form) {

        var merchant = {
            DECName: $scope.MerchantName,

            button1_text: $scope.button1_text,
            button2_text: $scope.button2_text,
            button2_url: $scope.button2_url,
            button3_text: $scope.button3_text,
            button3_url: $scope.button3_url,
            button4_text: $scope.button4_text,


            BuildingName: $scope.BuildingName,
            SocietyName: $scope.SocietyName,
            Street: $scope.Street,
            Country: $scope.countryid,
            State: $scope.stateid,

            Location: $scope.Location,
            City: $scope.City,

            PinCode: $scope.PinCode,
            Email: $scope.Email,
            Category: $scope.categoryid,

            PhoneNumber: $scope.PhoneNumber,
            MerchantLogo: $scope.merchantlogodata,
            MerchantDEC: $scope.merchantdecdata,
            merchantDecFromLibrary: $scope.SelectedImageSrc,
            RewardRs: $scope.RewardRs,
            RewardPoints: $scope.RewardPts,
            RedeemRs: $scope.RedeemRs,
            RedeemPt: $scope.RedeemPt,
            JoiningBonus: $scope.JoiningBonus
        };

        if ($scope.checkboxSelection == "no") {
            merchant.RunRewardProgram = false;
        }
        else {
            merchant.RunRewardProgram = true;
        }
        var redeemoptions =
            {
                Option1: $scope.condition1,
                Option2: $scope.condition2,
                Option3: $scope.condition3,
                Option4: $scope.condition4,
                Option5: $scope.condition5,
            }

        merchant.redeemoptions = redeemoptions;

        merchant.MerchantName = $scope.MerchantName;
        merchant.DECName = $scope.DECName;
        merchant.merchantid = $scope.merchantid;
        merchant.UserId = $scope.UserId;
        merchant.PhoneNumber = $scope.PhoneNumber;

        var getData = angularService.updateMerchant(merchant);
        getData.then(function (msg) {
            alert(msg.data);
            $rootScope.divEditMerchant = false;
            $rootScope.divSearch = true;
        }, function (err) {
            if (err != null) {
                if (err.data != null) {
                    alert(err.data);
                    alert('Error in updating merchant');
                }
            }

        });

    }

    function ClearMerchantFields() {
        $scope.merchantid = "";
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

    //To Get All Records  
    function GetAllMerchants() {

        $scope.divMerchant = false;
        $scope.divMerchants = true;

        var getData = angularService.getMerchants();
        getData.then(function (mch) {

            $scope.merchants = mch.data;

        }, function () {
            alert('Error in getting records GetAllMerchants');
        });
    }

    /* --------------------------------------------------------------------- */

    $scope.file_changed = function (element) {
        $scope.$apply(function (scope) {
            var photofile = element.files[0];
            var reader = new FileReader();
            reader.onload = function (e) {
                // handle onload
                $scope.data = btoa(e.target.result);
            };
            reader.readAsDataURL(element.files[0]);
            //reader.readAsBinaryString(photofile);
        });
    };

    $scope.logofile_changed = function (element) {
        $scope.$apply(function (scope) {

            var photofile = element.files[0];
            var reader = new FileReader();
            reader.onload = function (e) {
                // handle onload
                $scope.logodata = btoa(e.target.result);
            };
            reader.readAsDataURL(element.files[0]);
            //reader.readAsBinaryString(photofile);
        });
    };

    //To Get All Records  
    function GetAllBanks() {

        var getData = angularService.getBanks();
        getData.then(function (bnk) {
            $scope.banks = bnk.data;
        }, function () {
            alert('Error in getting records GetAllBanks');
        });
    }

    function getBankDetails(bankid) {

        var getData = angularService.getBank(bank.bankid);
        getData.then(function (bnk) {
            $scope.bank = bnk.data;
            $scope.bankid = bnk.bankid;
            $scope.bankname = bnk.bankname;
            $scope.button1_text = bnk.button1_text;
            $scope.button1_url = bnk.button1_url;
            $scope.button2_text = bnk.button2_text;
            $scope.button2_url = bnk.button2_url;
            $scope.button3_text = bnk.button3_text;
            $scope.button3_url = bnk.button3_url;
            $scope.button4_text = bnk.button4_text;
            $scope.button4_url = bnk.button4_url;

            $scope.Action = "Update";
        }, function () {
            alert('Error in getting records getBankDetails');
        });
    }



    $scope.AddUpdateBank = function (form) {

        if ($scope[form].$valid) {
            if ($scope.bankname == undefined) {
                alert("Please enter bank name.");
                return;
            }
            else if ($scope.button1_text == undefined) {
                alert("Please enter button1 text.");
                return;
            }
            else if ($scope.button1_url == undefined) {
                alert("Please enter button1 url.");
                return;
            }
            else if ($scope.button2_text == undefined) {
                alert("Please enter button2 text.");
                return;
            }
            else if ($scope.button2_url == undefined) {
                alert("Please enter button2 url.");
                return;
            }
            else if ($scope.button3_text == undefined) {
                alert("Please enter button3 text.");
                return;
            }
            else if ($scope.button3_url == undefined) {
                alert("Please enter button3 url.");
                return;
            }
            else if ($scope.button4_text == undefined) {
                alert("Please enter button4 text.");
                return;
            }
            else if ($scope.button4_url == undefined) {
                alert("Please enter button4 url.");
                return;
            }


            var bank = {
                bankname: $scope.bankname,
                button1_text: $scope.button1_text,
                button1_url: $scope.button1_url,
                button2_text: $scope.button2_text,
                button2_url: $scope.button2_url,
                button3_text: $scope.button3_text,
                button3_url: $scope.button3_url,
                button4_text: $scope.button4_text,
                button4_url: $scope.button4_url,
                bank_logo: $scope.logodata
            };


            //var getAction = "Add";
            var getAction = $scope.Action;

            if (getAction == "Update") {
                if ($scope.DECName == undefined) {
                    alert("Please enter bank DEC name.");
                    return;
                }
                bank.bankid = $scope.bankid;
                var bank_dec = {
                    decname: $scope.DECName,
                    decimage: $scope.data
                }
                bank.bank_dec_details = bank_dec;

                var getData = angularService.updateBank(bank);
                getData.then(function (msg) {
                    alert(msg.data);
                    ClearFields();
                }, function () {
                    alert('Error in updating record');
                });
            } else {
                if ($scope.bank_dec_details.decname == undefined) {
                    alert("Please enter bank DEC name.");
                    return;
                }
                var bank_dec = {
                    decname: $scope.bank_dec_details.decname,
                    decimage: $scope.data
                }
                bank.bank_dec_details = bank_dec;

                var getData = angularService.AddBank(bank);
                getData.then(function (msg) {
                    alert(msg.data);
                    ClearFields();
                }, function () {
                    alert('Error in adding record');
                });
            }
        }
        else {
            $scope.showMsgs = true;
        }
    }


    function ClearFields() {
        $scope.bankid = "";
        $scope.bankname = "";
        $scope.button1_text = "Coupons";
        $scope.button1_url = "";
        $scope.button2_text = "Redeem";
        $scope.button2_url = "";
        $scope.button3_text = "Book/Order";
        $scope.button3_url = "";
        $scope.button4_text = "Bank Benefits";
        $scope.button4_url = "";
        $scope.data = 'none';
        $scope.decname = "";
        $scope.decimage = "";
    }

    $scope.NavigateBack = function () {
        //Go back to home page

        if ($rootScope.divSupport == true) {
            $rootScope.showFooter = false;
            $rootScope.divSupport = false;
            $rootScope.divVAR = false;
            $rootScope.divAdminIndex = true;
        }
        else if ($rootScope.divVAR == true) {
            $rootScope.showFooter = false;
            $rootScope.divSupport = false;
            $rootScope.divVAR = false;
            $rootScope.divAdminIndex = true;
        }
        else if ($rootScope.divSearch == true) {
            window.history.back();
        }
        else if ($rootScope.divEditMerchant == true) {
            $rootScope.divSearch = true;
            $rootScope.divEditMerchant = false;
            $rootScope.divImageGallery = false;
            $rootScope.divEnlargeImage = false;
        }
        else if ($rootScope.divImageGallery == true) {
            $rootScope.divSearch = false;
            $rootScope.divEditMerchant = true;
            $rootScope.divImageGallery = false;
            $rootScope.divEnlargeImage = false;
        }
        else if ($rootScope.divEnlargeImage == true) {
            $rootScope.divSearch = false;
            $rootScope.divEditMerchant = false;
            $rootScope.divImageGallery = true;
            $rootScope.divEnlargeImage = false;
        }
        else {
            window.history.back();
        }
    }

    $scope.SearchMerchant = function (form) {

        if ($scope[form].$valid) {
            if (($scope.ValidFrom != undefined && $scope.ValidTill != undefined) && ($scope.ValidFrom > $scope.ValidTill)) {
                alert('From date must be less than to date');
            }
            else {
                $rootScope.searchButtonText = true;
                var searchParameter = {
                    Country: $scope.Country,
                    City: $scope.SearchCity,
                    State: $scope.State,
                    Pin: $scope.PIN,
                    VAR: $scope.VARCode,
                    Srep: $scope.rname,
                    Mobile: $scope.MobileNumber,
                    Business: $scope.BusinessName,
                    ValidFrom: $scope.ValidFrom,
                    ValidTill: $scope.ValidTill
                };

                var getData = angularService.searchMerchants(searchParameter);
                getData.then(function (mch) {
                    $rootScope.searchButtonText = false;
                    $scope.merchants = mch.data;
                }, function (errdata) {
                    $rootScope.searchButtonText = false;
                    if (errdata != null) {
                        if (errdata.data != null) {
                            alert(errdata.data);
                            alert('Error in searching records SearchMerchant');
                        }
                    }
                });
            }
        }
    }



    $scope.selectedAll = function () {
        $scope.merchants.forEach(function (x) {
            if ($scope.selectAll) {
                x.selected = true;
            }
            else {
                x.selected = false;
            }
        });
    }

    $scope.activateSelected = function () {
        $scope.activateMerchantId = "";
        for (var k = 0; k < $scope.merchants.length; k++) {
            if ($scope.merchants[k].selected == true) {
                //Perform your desired thing over here
                $scope.activateMerchantId = $scope.activateMerchantId + $scope.merchants[k].merchantid + ",";
            }
        }

        if ($scope.activateMerchantId != "") {
            var getData = angularService.activateMerchant($scope.activateMerchantId);
            getData.then(function (mch) {
                window.location.href = '/Sarvatra/CusomerSupport';
                alert(mch.data);

            }, function () {
                alert('Error in getting records activateMerchantId');
            });
        }
    }


    $scope.showBankSettingsPanel = function () {


        $scope.activateMerchantCount = 0;
        if ($scope.merchants == undefined) {
            alert('Please click on search to get merchant list.');
        }
        else if ($scope.merchants == null) {
            alert('Please click on search to get merchant list.');
        }
        else if ($scope.merchants.length == 0) {
            alert('No merchants exists matching the search criteria.')
        }
        else {


            for (var k = 0; k < $scope.merchants.length; k++) {
                if ($scope.merchants[k].selected == true) {
                    //Perform your desired thing over here
                    $scope.activateMerchantCount = $scope.activateMerchantCount + 1;
                }
            }

            if ($scope.activateMerchantCount == 0) {
                alert("Please select at least one merchant to set bank benefits.");
            }
            else {
                $rootScope.divSearch = false;
                $rootScope.divSetBankBenefits = true;
            }
        }
    }

    $scope.SetBenefits = function () {
        $scope.activateMerchantCount = 0;
        $scope.activateMerchantId = "";

        if ($scope.merchants == undefined) {
            alert('Please click on search to get merchant list.');
        }
        else if ($scope.merchants == null) {
            alert('Please click on search to get merchant list.');
        }
        else if ($scope.merchants.length == 0) {
            alert('No merchants exists matching the search criteria.')
        }
        else if ($scope.benefits == undefined) {
            alert('Please add benefits.');
        }
        else if ($scope.benefits == null) {
            alert('Please add benefits.');
        }
        else if ($scope.benefits.length == 0) {
            alert('Please add benefits.')
        }
        else {
            $scope.bank_benefits = [];
            
            for (var k = 0; k < $scope.merchants.length; k++) {
                if ($scope.merchants[k].selected == true) {
                    //Perform your desired thing over here
                    $scope.activateMerchantCount = $scope.activateMerchantCount + 1;
                    for (var i = 0; i < $scope.benefits.length; i++) {
                        var bbenefit = {
                            BankName: $scope.benefits[i].BankName,
                            Benefit: $scope.benefits[i].Benefit,
                            URL: $scope.benefits[i].URL,
                            MerchantId: $scope.merchants[k].merchantid
                        };

                        $scope.bank_benefits.push(bbenefit);
                    }
                }
            }
            
            if ($scope.activateMerchantCount == 0) {
                alert("No merchant selected to set bank benefits.");
            }
            else {
                
                var getData = angularService.SetBenefits($scope.bank_benefits);
                getData.then(function (mch) {
                    alert(mch.data);
                    $rootScope.divSearch = true;
                    $rootScope.divSetBankBenefits = false;

                }, function () {
                    alert('Error in setting benefits SetBenefits');
                    $rootScope.divSearch = true;
                    $rootScope.divSetBankBenefits = false;

                });
            }


        }
    }

    $scope.benefits = [];

    $scope.AddBenefit = function () {
        if ($scope.BankName == undefined || $scope.BankName == "") {
            alert('Please enter bank name');
        }
        else if ($scope.Benefit == undefined || $scope.Benefit == "") {
            alert('Please enter benefit description');
        }
        else if ($scope.URL == undefined || $scope.URL == "") {
            alert('Please enter URL');
        }
        else {
            

            var benefit = {
                BankName: $scope.BankName,
                Benefit: $scope.Benefit,
                URL: $scope.URL,
                MerchantId: 0
            };

            $scope.benefits.push(benefit);
            $scope.BankName = "";
            $scope.Benefit = "";
            $scope.URL = "";
        }
    }


    $scope.setDiscountToSelected = function () {

        if ($scope.GiftDiscount == undefined) {
            alert('Please enter End Customer Discount');
        }
        else if ($scope.GiftDiscount == "") {
            alert('Please enter End Customer Discount');
        }
        else {
            $scope.activateMerchantId = "";
            for (var k = 0; k < $scope.merchants.length; k++) {
                if ($scope.merchants[k].selected == true) {
                    //Perform your desired thing over here
                    $scope.activateMerchantId = $scope.activateMerchantId + $scope.merchants[k].merchantid + ",";
                }
            }

            if ($scope.activateMerchantId != "") {
                var getData = angularService.setDiscountForMerchant($scope.activateMerchantId, $scope.GiftDiscount);
                getData.then(function (mch) {
                    window.location.href = '/Sarvatra/GiftCardSettings';
                    alert(mch.data);

                }, function () {
                    alert('Error in getting records setDiscountToSelected');
                });
            }
        }
    }

    $scope.deactivateSelected = function () {

        $scope.activateMerchantId = "";
        for (var k = 0; k < $scope.merchants.length; k++) {
            if ($scope.merchants[k].selected == true) {
                //Perform your desired thing over here
                $scope.activateMerchantId = $scope.activateMerchantId + $scope.merchants[k].merchantid + ",";
            }
        }

        if ($scope.activateMerchantId != "") {
            var getData = angularService.deactivateMerchant($scope.activateMerchantId);
            getData.then(function (mch) {
                alert(mch.data);
                window.location.href = '/Sarvatra/CusomerSupport';
            }, function () {
                alert('Error in getting records deactivateSelected');
            });
        }
    }

    $scope.editSeleted = function () {
        $scope.selectedMerchantId = 0;
        var selectcount = 0;
        for (var k = 0; k < $scope.merchants.length; k++) {
            if ($scope.merchants[k].selected == true) {
                //Perform your desired thing over here
                $scope.selectedMerchantId = $scope.merchants[k].merchantid;
                selectcount += 1;
                if (selectcount > 1) {
                    $scope.selectedMerchantId = 0;
                    alert("Select only one merchant to edit.");
                    break;
                }
            }
        }
        if ($scope.selectedMerchantId != 0) {
            fillBusinessCategoryList();
            var getData = angularService.getMerchant($scope.selectedMerchantId);
            getData.then(function (mch) {
                $rootScope.divEditMerchant = true;
                $rootScope.divSearch = false;

                $scope.merchant = mch.data;
                $scope.merchantid = mch.data.merchantid;
                $scope.UserId = mch.data.UserId;
                $scope.MerchantName = mch.data.MerchantName;
                $scope.DECName = mch.data.DECName;
                $scope.BuildingName = mch.data.BuildingName;
                $scope.SocietyName = mch.data.SocietyName;
                $scope.Street = mch.data.Street;
                $scope.button1_text = mch.data.button1_text;

                $scope.button2_text = mch.data.button2_text;
                $scope.button2_url = mch.data.button2_url;

                $scope.button3_text = mch.data.button3_text;
                $scope.button3_url = mch.data.button3_url;

                $scope.button4_text = mch.data.button4_text;

                $scope.countryid = mch.data.Country;
                $scope.stateid = mch.data.State;

                $scope.Location = mch.data.Location;
                $scope.City = mch.data.City;
                //$scope.Country = mch.data.Country;
                //$scope.State = mch.data.State;

                $scope.PinCode = mch.data.PinCode;
                $scope.Email = mch.data.Email;
                $scope.categoryid = mch.data.Category;
                $scope.PhoneNumber = mch.data.PhoneNumber;
                GetCountryCode();
                $scope.lblCountryCode = "91";
                $scope.JoiningBonus = mch.data.JoiningBonus;

                if (mch.data.UserName.indexOf('@') > 0) {
                    $scope.usernameSelection = 'email';
                }
                else {
                    $scope.usernameSelection = 'phone';
                }

                if (mch.data.RunRewardProgram != null) {
                    if (mch.data.RunRewardProgram == false) {
                        $scope.checkboxSelection = 'no';
                        $scope.condition1 = mch.data.redeemoptions.Option1;
                        $scope.condition2 = mch.data.redeemoptions.Option2;
                        $scope.condition3 = mch.data.redeemoptions.Option3;
                        $scope.condition4 = mch.data.redeemoptions.Option4;
                        $scope.condition5 = mch.data.redeemoptions.Option5;
                    }
                    else {
                        $scope.checkboxSelection = 'yes';
                    }
                }


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

                $scope.selectedcatobject = { categoryid: $scope.categoryid };

                $scope.selectedcountryobject = { countryid: $scope.countryid };
                $scope.selectedstateobject = { stateid: $scope.stateid };

                $scope.getStates($scope.selectedcountryobject);
                $scope.RewardRs = mch.data.RewardRs;
                $scope.RewardPts = mch.data.RewardPoints;

                $scope.RedeemPt = mch.data.RedeemPt;
                $scope.RedeemRs = mch.data.RedeemRs;

                //Get reward formula of merchant
                //var getRewardData = angularService.getMerchantRewards(mch.data.UserId);
                //getRewardData.then(function (rwd) {

                //    $scope.RewardRs = rwd.data.RewardRs;
                //    $scope.RewardPts = rwd.data.RewardPoints;
                //});

                ////Get reward formula of merchant
                //var getRedeemData = angularService.getMerchantRedeems(mch.data.UserId);
                //getRedeemData.then(function (red) {

                //    $scope.RedeemPt = red.data.RedeemPt;
                //    $scope.RedeemRs = red.data.RedeemRs;
                //});
            }, function (errdata) {
                if (errdata != null) {
                    if (errdata.data != null) {
                        alert(errdata.data);
                        alert('Error in getting records editMerchant');
                    }
                }
            });
        }
    }

    //Link Bank and Consumer
    //PA-12-07-2017
    //Sending Coupons to Consumer
    $scope.LinkBanktoConsumer = function (form) {

        if ($scope[form].$valid) {
            if ($scope.MobileNo == undefined) {
                alert('Please enter Mobile Number');
            }
            else {
                var MobileNo = $scope.MobileNo;
                var BankId = document.getElementsByName('hdnBankId')[0].value;
                var getData = angularService.AddCheckBankConsumer(MobileNo, BankId);
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
                        alert(err.data);
                        alert('Error in Sending Mobile No');
                    }
                });
            }

        } else {
            $scope.showMsgs = true;
        }
    }

});