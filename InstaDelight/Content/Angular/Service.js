app.service("angularService", function ($http) {
    // Update Bank 
    this.updateBank = function (bank) {
        var response = $http({
            method: "post",
            url: "Bank/UpdateBank",
            data: JSON.stringify(bank),
            dataType: "json"
        });
        return response;
    }

    // Add Bank
    this.AddBank = function (bank) {
        var response = $http({
            method: "post",
            url: "/Bank/AddNewBank",
            data: JSON.stringify(bank),
            dataType: "json"
        });
        return response;
    }



    this.getCountryFromCountryCode = function (countrycode) {
        var response = $http({
            method: "post",
            url: "/Merchant/GetCountryFromcode",
            data: { countrycode: countrycode }
        });
        return response;
    }



    //get All banks
    this.getBanks = function () {
        // return $http.get("/Bank/GetAll");
        var response = $http({
            method: "post",
            url: "/Bank/GetAll"
        });
        return response;
    };

    // get bank By Id
    this.getBank = function (bankid) {
        var response = $http({
            method: "post",
            url: "/Bank/GetBankById",
            params: {
                bankid: JSON.stringify(bankid)
            }
        });
        return response;
    }

    //Link Bank & Consumer
    this.AddCheckBankConsumer = function (mobileno, BankId) {

        var response = $http({
            method: "post",
            url: "/Bank/AddNewBankConsumer",
            params: {
                mobileno: mobileno,
                BankId: BankId
            }
        });
        return response;
    }

    this.getBankDEC = function (bankid) {
        var response = $http({
            method: "post",
            url: "/Bank/getBankDEC",
            params: {
                bankid: bankid
            }
        });
        return response;
    }

    /*--------------------------------------------------------------*/
    //Merchant
    //Get business category
    this.getBusinessCategory = function () {
        return $http.get("/Merchant/GetCategories");
    }

    //Get Countries
    this.getCountries = function () {
        var response = $http({
            method: "post",
            url: "/Merchant/GetCountries",
        });
        return response;
    }

    this.getCurrency = function () {
        var response = $http({
            method: "post",
            url: "/Merchant/getCurrency"
        });
        return response;
    }

    this.getStates = function (countryid) {
        var response = $http({
            method: "post",
            url: "/Merchant/GetStates",
            data: {
                countryid: countryid
            }
        });
        return response;
    }

    this.getCountrycode = function (countryid) {
        var response = $http({
            method: "post",
            url: "/Merchant/GetCountrycode",
            data: { countryid: countryid },
            dataType: "json"
        });
        return response;
    };


    this.getCities = function () {
        return $http.get("/Merchant/GetCities");
    }

    this.getLocations = function (cityid) {
        var response = $http({
            method: "post",
            url: "/Merchant/GetLocations",
            params: {
                cityid: JSON.stringify(cityid)
            }
        });
        return response;
    }


    //get All banks
    this.getMerchants = function () {
        var response = $http({
            method: "post",
            url: "/Merchant/GetAll"
        });
        return response;
    };

    this.getVARCodes = function () {
        return $http.get("/Sarvatra/getVARCodes");
    }

    this.getVARUsers = function () {
        var response = $http({
            method: "post",
            url: "/Sarvatra/GetVARUsers"
        });
        return response;
    }

    this.getSupportUsers = function () {
        var response = $http({
            method: "post",
            url: "/Sarvatra/GetSupportUsers"
        });
        return response;
    }

    this.CreateVAR = function (merchant) {
        var response = $http({
            method: "post",
            url: "/Sarvatra/AddNewVAR",
            data: JSON.stringify(merchant),
            dataType: "json"
        });
        return response;
    }

    this.EditVAR = function (merchant) {
        var response = $http({
            method: "post",
            url: "/Sarvatra/EditVAR",
            data: JSON.stringify(merchant),
            dataType: "json"
        });
        return response;
    }

    this.EditVARStaff = function (merchant) {
        var response = $http({
            method: "post",
            url: "/Sarvatra/EditVARUser",
            data: JSON.stringify(merchant),
            dataType: "json"
        });
        return response;

    }
    this.CreateVARStaff = function (merchant) {
        var response = $http({
            method: "post",
            url: "/Sarvatra/AddNewVARStaff",
            data: JSON.stringify(merchant),
            dataType: "json"
        });
        return response;
    }

    this.activateVAR = function (userid) {
        var response = $http({
            method: "post",
            url: "/Sarvatra/ChangeVARStatus",
            data: {
                userid: userid,
                action: "Activate"
            }
        });
        return response;
    }

    this.deactivateSelectedVAR = function (userid) {
        var response = $http({
            method: "post",
            url: "/Sarvatra/ChangeVARStatus",
            data: {
                userid: userid,
                action: "Deactivate"
            }
        });
        return response;
    }

    // Add Merchant
    this.AddMerchant = function (merchant) {
        var response = $http({
            method: "post",
            url: "/Merchant/AddNewMerchant",
            data: JSON.stringify(merchant),
            dataType: "json"
        });
        return response;
    }

    // Update merchant 
    this.updateMerchant = function (merchant) {
        var response = $http({
            method: "post",
            url: "/Merchant/UpdateMerchant",
            data: JSON.stringify(merchant),
            dataType: "json"
        });
        return response;
    }

    // get bank By Id
    this.getMerchant = function (merchantid) {

        var response = $http({
            method: "post",
            url: "/Merchant/GetMerchantById",
            params: {
                merchantid: JSON.stringify(merchantid)
            }
        });
        return response;
    }

    this.editMerchant = function (merchantid) {

        var response = $http({
            method: "post",
            url: "/Sarvatra/EditMerchant",
            params: {
                merchantid: JSON.stringify(merchantid)
            }
        });
        return response;
    }

    this.getMerchantRewards = function (merchantid) {
        var response = $http({
            method: "post",
            url: "/Merchant/getMerchantRewards",
            params: {
                merchantid: merchantid
            }
        });
        return response;
    }

    this.activateMerchant = function (selectedMechant) {
        var response = $http({
            method: "post",
            url: "/Merchant/activateMerchants",
            params: {
                merchantids: selectedMechant
            }
        });
        return response;
    }



    this.deactivateMerchant = function (selectedMechant) {
        var response = $http({
            method: "post",
            url: "/Merchant/deActivateMerchants",
            params: {
                merchantids: selectedMechant
            }
        });
        return response;
    }

    this.setDiscountForMerchant = function (selectedMechant, discount) {
        var response = $http({
            method: "post",
            url: "/Merchant/SetGiftcardDiscount",
            data: {
                merchantids: selectedMechant,
                discount: discount
            }
        });
        return response;
    }

    this.SetBenefits = function (benefits) {
        var response = $http({
            method: "post",
            url: "/Merchant/SetBenefits",
            data: {
                benefits: benefits
            }
        });
        return response;
    }

    this.getMerchantRedeems = function (merchantid) {
        var response = $http({
            method: "post",
            url: "/Merchant/getMerchantRedeems",
            params: {
                merchantid: merchantid
            }
        });
        return response;
    }

    this.searchMerchants = function (SearchParameters) {
        var response = $http({
            method: "post",
            url: "/Merchant/SearchMerchants",
            data: JSON.stringify(SearchParameters),
            dataType: "json"
        });
        return response;
    }


    /*------------------------------------------------------------------*/
});