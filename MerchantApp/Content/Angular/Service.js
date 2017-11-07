app.service("angularService", function ($http) {
    var res = {
        'MobileNoPrompt': {
            'en': 'Please enter Mobile Number',
            'mr': 'कृपया मोबाइल नंबर प्रविष्ट करा',
            'hi': "कृपया मोबाइल नंबर दर्ज करें",
            'bn': "দয়া করে মোবাইল নম্বর লিখুন"
        },
        'SendOTPMessage': {
            'en': 'OTP sent successfully',
            'mr': 'OTP यशस्वीपणे पाठवला',
            'hi': "OTP सफलतापूर्वक भेजा गया",
            'bn': "OTP সফলভাবে পাঠানো হয়েছে"
        },
        'OTPVerified': {
            'en': 'OTP Verified',
            'mr': 'OTP सत्यापित आहे',
            'hi': "OTP सत्यापित है",
            'bn': "OTP যাচাই করা হয়েছে"
        },
        'MobileNoEmailPrompt': {
            'en': 'Please enter Mobile Number or Email',
            'mr': 'कृपया मोबाइल नंबर किंवा ईमेल प्रविष्ट करा',
            'hi': "कृपया मोबाइल नंबर या ईमेल दर्ज करें",
            'bn': "দয়া করে মোবাইল নম্বর বা ইমেল লিখুন"
        },
        'BillAmountPrompt': {
            'en': 'Please enter Bill Amount',
            'mr': 'कृपया बिल रक्कम प्रविष्ट करा',
            'hi': "कृपया बिल राशि दर्ज करें",
            'bn': "দয়া করে বিল পরিমাণ লিখুন"
        },
        'CouponCodePrompt': {
            'en': 'Please enter coupon code',
            'mr': 'कृपया कूपन कोड प्रविष्ट करा',
            'hi': "कृपया कूपन कोड दर्ज करें",
            'bn': "কুপন কোড লিখুন দয়া করে"
        },
        'DateTimePrompt': {
            'en': 'From date must be greater than today',
            'mr': 'प्रारंभ तारीख आजपेक्षा मोठी असणे आवश्यक आहे',
            'hi': "प्रारंभ तिथि आज की तुलना में अधिक होनी चाहिए",
            'bn': "শুরুর তারিখটি আজকের চেয়ে বেশি হওয়া আবশ্যক"
        },
        'CouponAddedPrompt': {
            'en': 'Coupon Added Successfully',
            'mr': 'कुपन यशस्वीरित्या तयार केले गेले आहे',
            'hi': "कूपन सफलतापूर्वक जोड़ा गया",
            'bn': "কুপন সফলভাবে যোগ করা হয়েছে"
        },
        'NoPointsPrompt': {
            'en': 'No points to redeem',
            'mr': 'पूर्तता करण्यासाठी कोणतेही गुण नाहीत',
            'hi': "रिडीम करने के लिए कोई अंक नहीं",
            'bn': "মুক্তির কোন পয়েন্ট নেই"
        },
        'EnterPointsPrompt': {
            'en': 'Please enter points to be redeemed',
            'mr': 'कृपया पूर्तता करण्याचे गुण प्रविष्ट करा',
            'hi': "कृपया रिडीम करने के लिए अंक दर्ज करें",
            'bn': "প্রত্যাহার করা পয়েন্ট লিখুন দয়া করে"
        },
        'MaxPointsPrompt': {
            'en': 'Points to be redeemed should be less than available points',
            'mr': 'पूर्तता करण्याचे पॉइंट उपलब्ध पॉइंटपेक्षा कमी असले पाहिजेत',
            'hi': "रिडीम करने के लिए अंक उपलब्ध अंक से कम होना चाहिए",
            'bn': "প্রত্যাশিত পয়েন্টগুলি উপলব্ধ পয়েন্টগুলি থেকে কম হওয়া উচিত"
        },
        'CreateBrand': {
            'en': 'Create a New Brand',
            'mr': 'एक नवीन ब्रँड तयार करा',
            'hi': "एक नया ब्रांड बनाएं",
            'bn': "একটি নতুন ব্র্যান্ড তৈরি করুন"
        },
        'UpdateBrand': {
            'en': 'Update brand details',
            'mr': 'ब्रँड तपशील अद्यतनित करा',
            'hi': "ब्रांड विवरण अपडेट करें",
            'bn': "ব্র্যান্ড বিবরণ আপডেট করুন"
        },
        'SelectBrand': {
            'en': 'Please Select Brand',
            'mr': 'कृपया ब्रँड निवडा',
            'hi': "कृपया ब्रांड का चयन करें",
            'bn': "ব্র্যান্ড নির্বাচন করুন"
        },
        'SelectBranch': {
            'en': 'Please Select Location',
            'mr': 'कृपया स्थान निवडा',
            'hi': "कृपया स्थान का चयन करें",
            'bn': "অবস্থান নির্বাচন করুন"
        },
        'CreateLocation': {
            'en': 'Create a New Location',
            'mr': 'एक नवीन स्थान तयार करा',
            'hi': "एक नया स्थान बनाएं",
            'bn': "একটি নতুন অবস্থান তৈরি করুন"
        },
        'UpdateLocation': {
            'en': 'Update Location Details',
            'mr': 'स्थान अद्यतनित करा',
            'hi': "स्थान अपडेट करें",
            'bn': "অবস্থান আপডেট করুন"
        },
        'CreateStaff': {
            'en': 'Add a New Staff Member',
            'mr': 'एक नवीन कर्मचारी सदस्य जोडा',
            'hi': "एक नया स्टाफ सदस्य जोड़ें",
            'bn': "একটি নতুন স্টাফ সদস্য যোগ করুন"
        },
        'UpdateStaff': {
            'en': 'Update Staff Member Details',
            'mr': 'स्टाफ सदस्य तपशील अद्यतनित करा',
            'hi': "स्टाफ़ सदस्य विवरण अपडेट करें",
            'bn': "স্টাফ সদস্যের বিবরণ আপডেট করুন"
        },
        'NoLicensesPrompt': {
            'en': 'Looks like you have run out of licenses!For adding more Brands or locations, buy more licenses...',
            'mr': 'असे दिसते की आपल्याकडे परवाने संपले आहेत! अधिक ब्रांड किंवा स्थाने जोडण्यासाठी, अधिक परवाने विकत घ्या ...',
            'hi': "ऐसा लगता है कि आपके पास कोई लाइसेंस नहीं है! अधिक ब्रांड या स्थान जोड़ने के लिए, अधिक लाइसेंस खरीदें ...",
            'bn': "মনে হচ্ছে আপনার কাছে কোনও লাইসেন্স নেই! আরো ব্র্যান্ড বা অবস্থান যোগ করার জন্য, আরও লাইসেন্স কিনুন ..."
        }
    }

    this.getString = function (key, locale) {
        return res[key][locale];
    };


    this.GetCouponFromCouponCode = function (ccode) {
        var response = $http({
            method: "post",
            url: "/Merchant/GetCouponFromCouponCode",
            data: {
                couponcode: ccode
            }
        });
        return response;
    };

    // Redeem Coupon
    this.RedeemCoupon = function (couponcode) {
        var response = $http({
            method: "post",
            url: "/Merchant/RedeemCouponFromCode",
            data: {
                couponcode: couponcode
            }
        });
        return response;
    }

    this.sendOTPForCoupon = function (sharedcouponid) {
        var response = $http({
            method: "post",
            url: "/Merchant/SendCouponOTP",
            data: {
                sharedcouponid: sharedcouponid
            }
        });
        return response;
    }

    this.getLicenses = function () {
        var response = $http({
            method: "post",
            url: "/Merchant/GetLicenses"
        });
        return response;
    }



    //Add coupon 
    this.AddCheckCouponConsumer = function (mobileno, CouponId) {
        var response = $http({
            method: "post",
            url: "/Merchant/AddNewCouponConsumer",
            data: {
                mobileno: mobileno,
                CouponId: CouponId
            }
        });
        return response;
    }

    //Send coupon 
    this.SendToAll = function (CouponId) {
        var response = $http({
            method: "post",
            url: "/Merchant/SendCouponToAll",
            data: {
                CouponId: CouponId
            }
        });
        return response;
    }

    this.getCountrycode = function () {
        return $http.get("/Merchant/GetCountrycode");
    };

    this.getCustomers = function () {
        return $http.get("/Merchant/GetCustomers");
    }

    this.getCountryFromCountryCode = function (countrycode) {
        var response = $http({
            method: "post",
            url: "/Merchant/GetCountryFromcode",
            data: { countrycode: countrycode }
        });
        return response;
    }

    this.getReview = function () {
        return $http.get("/Merchant/GetReview");
    }

    this.getGifts = function () {
        return $http.get("/Merchant/GetGiftCard");
    }

    this.GetGiftCondition = function (GiftCardId) {
        var response = $http({
            method: "post",
            url: "/Merchant/GetGiftConditions",
            data: {
                GiftCardId: GiftCardId
            }
        });
        return response;
    }
    // Redeem Coupon
    this.CheckCouonValidity = function (couponcode, BillAmount, CustPhoneNumber) {
        var response = $http({
            method: "post",
            url: "/Merchant/RedeemCoupon",
            data: {
                couponcode: couponcode,
                billedamount: BillAmount,
                CustPhoneNumber: CustPhoneNumber
            }
        });
        return response;
    }

    this.GetConsumerCoupons = function (CustPhoneNumber) {
        var response = $http({
            method: "post",
            url: "/Merchant/GetMerchantCoupons",
            data: {
                ConsumerPhone: CustPhoneNumber
            }
        });
        return response;
    }

    this.CalculatePointToRs = function (RedeemPoints, CustPhoneNumber) {
        var response = $http({
            method: "post",
            url: "/Merchant/RedeemPoints",
            data: {
                RedeemPoints: RedeemPoints,
                CustPhoneNumber: CustPhoneNumber
            }
        });
        return response;
    }

    this.getNoOfVisits = function (CustPhoneNumber) {
        var response = $http({
            method: "post",
            url: "/Merchant/getNoOfVisits",
            data: {
                ConsumerPhone: CustPhoneNumber
            }
        });
        return response;
    }

    //Final redeem
    this.FinalRedeem = function (CouponId, SharedCouponId) {
        var response = $http({
            method: "post",
            url: "/Merchant/FinalRedeem",
            data: {
                CouponId: CouponId,
                SharedCouponId: SharedCouponId
            }
        });
        return response;
    }

    this.FinalGiftCardRedeem = function (Id) {
        var response = $http({
            method: "post",
            url: "/Merchant/FinalGiftcardRedeem",
            data: {
                Id: Id
            }
        });
        return response;
    }

    this.FinalRedeemPoints = function (RedeemPoints, custphonenumber) {
        var response = $http({
            method: "post",
            url: "/Merchant/FinalRedeemPoints",
            data: {
                RedeemPoints: RedeemPoints,
                custphonenumber: custphonenumber
            }
        });
        return response;
    }

    // Set Event Coupon
    this.SetCoupons = function (eventcoupons) {
        var response = $http({
            method: "post",
            url: "/Merchant/SetEventCoupon",
            data: {
                birthdaycoupon: eventcoupons.birthdaycoupon,
                anncoupon: eventcoupons.anncoupon,
                reviewcoupon: eventcoupons.reviewcoupon,
                sharecoupon: eventcoupons.sharecoupon
            }
        });
        return response;

    }

    //get All banks
    this.getCoupons = function () {
        // return $http.get("/Bank/GetAll");
        var response = $http({
            method: "post",
            url: "/Merchant/GetAllCoupons"
        });
        return response;
    };

    this.getGroups = function () {
        var response = $http({
            method: "post",
            url: "/Merchant/GetAllGroups"
        });
        return response;
    };

    this.getEventsCoupons = function () {
        var response = $http({
            method: "post",
            url: "/Merchant/GetAllEventsCoupons"
        });
        return response;
    }

    this.getExpPeriod = function () {
        var response = $http({
            method: "post",
            url: "/Merchant/GetExpPeriods"
        });
        return response;
    }

    //Get Countries
    this.getCountries = function () {
        var response = $http({
            method: "post",
            url: "/Merchant/GetCountries"
        });
        return response;
    }

    this.getCities = function (stateid) {
        var response = $http({
            method: "post",
            url: "/Merchant/GetCities",
            data: {
                stateid: JSON.stringify(stateid)
            }
        });
        return response;
    };

    this.sendCouponToGroups = function (groupnames, couponid) {
        var response = $http({
            method: "post",
            url: "/Merchant/sendCouponToGroups",
            data: {
                groupnames: groupnames,
                couponid: couponid
            }
        });
        return response;
    };

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

    this.getLocationsForCoupons = function () {

        var response = $http({
            method: "post",
            url: "/Merchant/GetLocationsForCoupons"
        });

        return response;
        //return $http.get("/Merchant/GetLocationsForCoupons");
    }


    this.getLocations = function (cityid) {
        var response = $http({
            method: "post",
            url: "/Merchant/GetLocations",
            data: {
                cityid: JSON.stringify(cityid)
            }
        });
        return response;
    }

    this.getBusinessCategory = function () {
        return $http.get("/Merchant/GetCategories");
    }

    this.getBrands = function () {
        return $http.get("/Merchant/getBrands");
    }

    this.getCurrency = function () {
        var response = $http({
            method: "post",
            url: "/Merchant/getCurrency"
        });
        return response;
    }

    // Add Merchant
    this.AddCoupon = function (coupon) {

        var response = $http({
            method: "post",
            url: "/Merchant/AddNewCoupon",
            //url: "/MerchantWebService.ashx?action=AddNewCoupon&userid=d2185967-608b-46d1-b285-d033ad725409",
            data: coupon,
            dataType: "json"
        });
        return response;
    }

    this.AddBrand = function (merchant) {
        var response = $http({
            method: "post",
            url: "/Merchant/AddNewBrand",
            data: merchant,
            dataType: "json"
        });
        return response;
    }

    this.UpdateBrand = function (brand) {
        var response = $http({
            method: "post",
            url: "/Merchant/UpdateBrand",
            data: brand,
            dataType: "json"
        });
        return response;
    }

    this.AddBranch = function (merchant) {
        var response = $http({
            method: "post",
            url: "/Merchant/AddNewBranch",
            data: JSON.stringify(merchant),
            dataType: "json"
        });
        return response;
    }

    this.UpdateBranch = function (branch) {
        var response = $http({
            method: "post",
            url: "/Merchant/UpdateBranch",
            data: branch,
            dataType: "json"
        });
        return response;
    }

    this.AddStaff = function (merchant) {
        var response = $http({
            method: "post",
            url: "/Merchant/AddNewStaff",
            data: JSON.stringify(merchant),
            dataType: "json"
        });
        return response;
    }

    this.UpdateStaff = function (staff) {
        var response = $http({
            method: "post",
            url: "/Merchant/UpdateStaff",
            data: staff,
            dataType: "json"
        });
        return response;
    }


    // Update merchant 
    this.updateCoupon = function (coupon) {
        var response = $http({
            method: "post",
            url: "/Merchant/UpdateCoupon",
            //url: "/MerchantWebService.ashx?action=UpdateCoupon&userid=d2185967-608b-46d1-b285-d033ad725409",
            data: JSON.stringify(coupon),
            dataType: "json"
        });
        return response;
    }

    this.getBrand = function (BrandId) {
        var response = $http({
            method: "post",
            url: "/Merchant/GetBrandById",
            data: {
                BrandId: BrandId
            }
        });
        return response;
    }

    this.getBranch = function (BranchId) {
        var response = $http({
            method: "post",
            url: "/Merchant/GetBranchById",
            data: {
                BranchId: BranchId
            }
        });
        return response;
    }

    this.getStaff = function (StaffId) {
        var response = $http({
            method: "post",
            url: "/Merchant/GetStaffById",
            data: {
                StaffId: StaffId
            }
        });
        return response;
    }

    this.getBranches = function (BrandId) {
        var response = $http({
            method: "post",
            url: "/Merchant/GetBranches",
            data: {
                BrandId: BrandId
            }
        });
        return response;
    }

    // get bank By Id
    this.getCoupon = function (couponid) {
        var response = $http({
            method: "post",
            url: "/Merchant/GeCouponById",
            data: {
                couponid: couponid
            }
        });
        return response;
    }



    this.getEventConditions = function (couponid) {
        var response = $http({
            method: "post",
            url: "/Merchant/GetEventConditions",
            data: { couponid: couponid }
        });
        return response;
    }

    // get bank By Id
    this.getMerchant = function (MerchantId) {
        var response = $http({
            method: "post",
            url: "/Merchant/GetMerchantById",
            data: {
                MerchantId: JSON.stringify(MerchantId)
            }
        });
        return response;
    }

    this.getMerchantRewards = function (MerchantId) {
        var response = $http({
            method: "post",
            url: "/Merchant/getMerchantRewards",
            data: {
                MerchantId: MerchantId
            }
        });
        return response;
    }

    this.getMerchantRedeems = function (MerchantId) {
        var response = $http({
            method: "post",
            url: "/Merchant/getMerchantRedeems",
            data: {
                MerchantId: MerchantId
            }
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

    this.AddReview = function (review) {
        var response = $http({
            method: "post",
            url: "/Merchant/AddNewReview",
            //url: "/MerchantWebService.ashx?action=AddNewReview&userid=d2185967-608b-46d1-b285-d033ad725409",
            data: review,
            dataType: "json"
        });
        return response;
    }

    // CheckAndAddDECConsumer   
    this.AddCheckDECConsumer = function (mobileno, BillAmt) {

        var response = $http({
            method: "post",
            url: "/Merchant/AddNewDECConsumer",
            data: {
                mobileno: mobileno,
                BillAmt: BillAmt
            }
        });
        return response;
    }


    this.getLanguage = function (countryid) {
        var response = $http({
            method: "post",
            url: "/Merchant/getLanguage",
            data: {
                countryid: countryid
            }
        });
        return response;
    }

    this.getLanguageCode = function () {

        var response = $http({
            method: "post",
            url: "/Merchant/getMerchantLanguage"
        });
        return response;
    }
    this.SaveChangedCountry = function (countryid, langid) {

        var response = $http({
            method: "post",
            url: "/Merchant/SaveChangedCountry",
            data: {
                countryid: countryid,
                langid: langid
            }
        });
        return response;
    }

    //Set Gift Card
    this.SaveMerchantGiftCard = function (giftcard) {

        var response = $http({
            method: "post",
            url: "/Merchant/SaveMerchantGiftCard",
            data: JSON.stringify(giftcard),

            dataType: "json"
        });
        return response;
    }

    //Save Gift Card Conditions
    this.SaveGiftCardConditions = function (conditionelemnt) {
        var response = $http({
            method: "post",
            url: "/Merchant/SaveGiftCardConditions",
            //data: JSON.stringify(conditionelemnt),
            data: JSON.stringify(conditionelemnt),
            dataType: "json"
        });
        return response;
    }

    this.VerifyOTP = function (OTP, GiftCardId) {
        var response = $http({
            method: "post",
            url: "/Merchant/VerifyGiftcardOTP",
            data: {
                OTP: OTP,
                GiftCardId: GiftCardId
            }
        });
        return response;
    }

    this.VerifyCouponOTP = function (OTP, SharedCouponId) {
        var response = $http({
            method: "post",
            url: "/Merchant/VerifyCouponOTP",
            data: {
                OTP: OTP,
                SharedCouponId: SharedCouponId
            }
        });
        return response;
    }

    this.SendDECtoConsumerList = function () {
        $http({
            method: "post",
            url: "/Merchant/UploadConsumers",
            //data: JSON.stringify(conditionelemnt),
            data: { CouponId: "0" },
            dataType: "json"
        });
    }


});