app.service("angularService", function ($http) {
    var res = {
        'OTPVerified': {
            'en': 'OTP Verified',
            'mr': 'OTP सत्यापित आहे',
            'hi': "OTP सत्यापित है",
            'bn': "OTP যাচাই করা হয়েছে"
        },
        'SendOTPMessage': {
            'en': 'OTP sent successfully',
            'mr': 'OTP यशस्वीपणे पाठवला',
            'hi': "OTP सफलतापूर्वक भेजा गया",
            'bn': "OTP সফলভাবে পাঠানো হয়েছে"
        },
        'NocouponsMessage': {
            'en': 'No coupons are shared with you by this merchant!',
            'mr': 'या व्यवसायाद्वारे आपल्यासह कोणतेही कूपन सामायिक केलेले नाहीत!',
            'hi': "इस व्यापारी द्वारा आपके साथ कोई कूपन नहीं साझा किए गए हैं!",
            'bn': "এই বণিক দ্বারা কোন কুপন আপনার সাথে ভাগ করা হয় না!"
        },
        'ReviewRedirectMessage': {
            'en': 'Redirecting to Send DEC Page',
            'mr': 'DEC पृष्ठ पाठविण्यासाठी पुनर्निर्देशित करीत आहे',
            'hi': "डीईसी पृष्ठ भेजने के लिए रीडायरेक्ट करना",
            'bn': "ডিসি পাতা পাঠাতে পুনর্নির্দেশ"
        },
        'ReviewMessage': {
            'en': 'Thank you for your feedback. We have sent you a coupon as a token of appreciation.',
            'mr': 'आपल्या अभिप्रायाबद्दल धन्यवाद. आम्ही आपल्या प्रतिसादाबद्दल आपल्याला कौतुक म्हणून एक कूपन पाठवत आहोत.',
            'hi': "आपकी प्रतिक्रिया के लिए आपका धन्यवाद। हमने आपको प्रशंसा के प्रतीक के रूप में एक कूपन भेजा है",
            'bn': "আপনার প্রতিক্রিয়ার জন্য ধন্যবাদ. আমরা আপনাকে একটি কুপন প্রেরণ করেছি প্রশংসার চিহ্ন হিসাবে।"
        },
        'CityCategoryPrompt': {
            'en': 'Please select either city or category and click on search',
            'mr': 'कृपया शहर किंवा श्रेणी निवडा आणि शोधावर क्लिक करा',
            'hi': "कृपया शहर या श्रेणी का चयन करें और खोज पर क्लिक करें",
            'bn': "শহর বা বিভাগ নির্বাচন করুন এবং অনুসন্ধান ক্লিক করুন"
        },
        'MobileNoPrompt': {
            'en': 'Please enter Mobile Number',
            'mr': 'कृपया मोबाइल नंबर प्रविष्ट करा',
            'hi': "कृपया मोबाइल नंबर दर्ज करें",
            'bn': "দয়া করে মোবাইল নম্বর লিখুন"
        },
        'EmailMobileNoPrompt': {
            'en': 'Please enter either Mobile Number or Email',
            'mr': 'कृपया एकतर मोबाइल नंबर किंवा ईमेल प्रविष्ट करा',
            'hi': "कृपया मोबाइल नंबर या ईमेल दर्ज करें",
            'bn': "দয়া করে মোবাইল নম্বর অথবা ইমেল লিখুন"
        },
        'BusinessMobileNoPrompt': {
            'en': 'Please enter Business Mobile Number',
            'mr': 'कृपया व्यवसायाचा मोबाईल नंबर प्रविष्ट करा',
            'hi': "कृपया व्यापार का मोबाइल नंबर दर्ज करें",
            'bn': "দয়া করে ব্যবসায়ের মোবাইল নম্বর লিখুন"
        },
        'ShareDECMessage': {
            'en': 'DEC Shared successfully.',
            'mr': 'DEC यशस्वीरित्या सामायिक केले',
            'hi': "DEC को सफलतापूर्वक साझा किया गया",
            'bn': "DEC ভাগ সফলভাবে"
        }
    }

    this.getString = function (key, locale) {
        return res[key][locale];
    };

    this.getLanguageCode = function () {

        var response = $http({
            method: "post",
            url: "/Consumer/getConsumerLanguage"
        });
        return response;
    }

    //get All banks
    this.GetAllBanks = function () {
        return $http.get("/Consumer/GetAll");
    };

    this.getMerchants = function () {
        return $http.get("/Consumer/GetAllMerchants");
    }

    this.getPendingReview = function () {
        return $http.get("/Consumer/GetPendingReviews");

    }

    this.GetPendingReviewForMerchant = function (merchantid) {
        var response = $http({
            method: "post",
            url: "/Consumer/GetPendingReviewForMerchant",
            data: { merchantid: merchantid }
        });
        return response;
    }

    //Add Review Comment 
    this.AddReviewComment = function (ReviewId, MerchantId, Comment) {
        var response = $http({
            method: "post",
            url: "/Consumer/SaveReviewComment",
            data: {
                ReviewId: ReviewId,
                MerchantId: MerchantId,
                Comment: Comment
            }
        });
        return response;
    }

    this.getConsumerLogo = function () {
        return $http.get("/Consumer/getConsumerLogo");
    }

    this.getConsumerPoints = function (merchantid) {
        var response = $http({
            method: "post",
            url: "/Consumer/getConsumerPoints",
            data: { merchantid: merchantid }
        });
        return response;
    }

    this.sendOTPtoEmail = function (Email) {
        var response = $http({
            method: "post",
            url: "/Consumer/sendOTPtoEmail",
            data: { EmailId: Email }
        });
        return response;
    }

    this.VerifyEmailOTP = function (OTP) {
        var response = $http({
            method: "post",
            url: "/Consumer/VerifyEmailOTP",
            data: { OTP: OTP }
        });
        return response;
    }

    this.sendOTPtoPhone2 = function (Phone) {
        var response = $http({
            method: "post",
            url: "/Consumer/sendOTPtoPhone2",
            data: { PhoneNumber: Phone }
        });
        return response;
    }

    this.VerifyPhone2OTP = function (OTP) {
        var response = $http({
            method: "post",
            url: "/Consumer/Verifyphone2otp",
            data: { OTP: OTP }
        });
        return response;
    }

    this.sendOTPtoPhone3 = function (Phone) {
        var response = $http({
            method: "post",
            url: "/Consumer/sendOTPtoPhone3",
            data: { PhoneNumber: Phone }
        });
        return response;
    }

    this.VerifyPhone3OTP = function (OTP) {
        var response = $http({
            method: "post",
            url: "/Consumer/Verifyphone3otp",
            data: { OTP: OTP }
        });
        return response;
    }

    this.getConsumerGiftCards = function (merchantid) {
        var response = $http({
            method: "post",
            url: "/Consumer/GetConsumerGiftCards",
            data: {
                merchantid: merchantid
            }
        });
        return response;
    }

    //Get Countries
    this.getCountries = function () {
        return $http.get("/Consumer/GetCountries");
    }


    this.getCities = function (stateid) {
        var response = $http({
            method: "post",
            url: "/Consumer/GetCities",
            data: {
                stateid: stateid
            }
        });
        return response;
    };

    this.getLocations = function (cityid) {
        var response = $http({
            method: "post",
            url: "/Consumer/GetLocations",
            data: {
                cityid: cityid
            }
        });
        return response;
    }

    this.getStates = function (countryid) {
        var response = $http({
            method: "post",
            url: "/Consumer/GetStates",
            data: {
                countryid: countryid
            }
        });
        return response;
    }

    // get decdetails By bankId
    this.getBankDECDetails = function (bankid) {
        var response = $http({
            method: "post",
            url: "/Consumer/GetDECDetails",
            data: {
                bankid: bankid
            }
        });
        return response;
    };

    // get decdetails By merchantid
    this.getMerchantDECDetails = function (merchantid) {
        var response = $http({
            method: "post",
            url: "/Consumer/GetMerchantDECDetails",
            data: {
                merchantid: merchantid
            }
        });
        return response;
    };

    this.getCountrycode = function () {
        return $http.get("/Consumer/GetCountrycode");
    };

    this.getCountryFromCountryCode = function (countrycode) {
        var response = $http({
            method: "post",
            url: "/Consumer/GetCountryFromcode",
            data: { countrycode: countrycode }
        });
        return response;
    }

    // get coupondetails By couponId
    this.getCouponDetails = function (couponid) {
        var response = $http({
            method: "post",
            url: "/Consumer/GetCouponDetails",
            data: { couponid: couponid }
        });
        return response;
    };

    this.getEventConditions = function (couponid) {
        var response = $http({
            method: "post",
            url: "/Consumer/GetEventConditions",
            data: { couponid: couponid }
        });
        return response;
    }

    this.getCouponConditions = function (couponid) {
        var response = $http({
            method: "post",
            url: "/Consumer/getCouponConditions",
            data: { couponid: couponid }
        });
        return response;
    }

    //Add dec 
    this.AddCheckDECConsumer = function (mobileno) {
        var response = $http({
            method: "post",
            url: "/Consumer/AddNewDECConsumer",
            data: { mobileno: mobileno }
        });
        return response;
    }

    this.GetMerchantDEC = function (mobileno, businessmobileno) {
        var response = $http({
            method: "post",
            url: "/Consumer/GetDECFromMerchant",
            data: {
                mobileno: mobileno,
                businessmobileno: businessmobileno
            }
        });
        return response;
    }

    //Add dec 
    this.AddCheckCouponConsumer = function (mobileno, MerchantId, CouponId, Id) {
        var response = $http({
            method: "post",
            url: "/Consumer/AddCheckCouponConsumer",
            data: {
                mobileno: mobileno,
                MerchantId: MerchantId,
                CouponId: CouponId,
                SharedCouponId: Id
            }
        });
        return response;
    }

    this.CalculatePointToRs = function (RedeemPoints, CustPhoneNumber) {
        var response = $http({
            method: "post",
            url: "/Consumer/AddCheckCouponConsumer",
            data: {
                mobileno: mobileno,
                MerchantId: MerchantId,
                CouponId: CouponId,
                SharedCouponId: Id
            }
        });
        return response;
    }

    //get merchant details
    this.getMerchantDetails = function (merchantid) {
        var response = $http({
            method: "post",
            url: "/Consumer/GetMerchantDetails",
            data: {
                merchantid: JSON.stringify(merchantid)
            }
        });
        return response;
    };


    this.getBusinessCategory = function () {
        return $http.get("/Consumer/GetCategories");
    }

    this.GetCoupons = function (cityid, locationid, categoryid, bankid) {
        var response = $http({
            method: "post",
            url: "/Consumer/GetCoupons",
            data: { cityid: cityid, locationid: locationid, categoryid: categoryid, bankid: bankid }
        });

        return response;
    };

    this.GetMerchantCoupons = function (merchantid) {
        var response = $http({
            method: "post",
            url: "/Consumer/GetMerchantCoupons",
            data: {
                merchantid: merchantid
            }
        });

        return response;
    };



    // Update merchant 
    this.updateConsumer = function (consumer) {
        var response = $http({
            method: "post",
            url: "/Consumer/UpdateConsumer",
            //url: "/ConsumerWebService.ashx?action=UpdateConsumer",
            data: JSON.stringify(consumer),
            dataType: "json"
        });
        return response;
    }

    // get bank By Id
    this.getConsumer = function (consumerid) {
        var response = $http({
            method: "post",
            url: "/Consumer/GetConsumerById",
            data: {
                consumerid: consumerid
            }
        });
        return response;
    }

    this.SaveChangedCountry = function (countryid, langid) {

        var response = $http({
            method: "post",
            url: "/Consumer/SaveChangedCountry",
            data: {
                countryid: countryid,
                langid: langid
            }
        });
        return response;
    }

    this.getLanguage = function (countryid) {
        var response = $http({
            method: "post",
            url: "/Consumer/getLanguage",
            data: {
                countryid: countryid
            }
        });
        return response;
    }


    //Reviews
    this.getallReview = function () {
        return $http.get("/Consumer/GetAllReviews");
    }

    this.getpendingReview = function () {
        return $http.get("/Consumer/GetPendingReviews");
    }

    //Save Consumer review
    this.SaveConsumerReview = function (reviewsubmit) {

        var response = $http({
            method: "post",
            url: "/Consumer/SaveConsumerReview",
            // url: "/ConsumerWebService.ashx",
            data: JSON.stringify(reviewsubmit),
            dataType: "json"
        });
        return response;
    }

    this.getPendingReview = function (consumerid) {

        return $http.get("/Consumer/GetPendingReviews");
    }

    this.getGiftDenomination = function (consumerid) {
        return $http.get("/Consumer/GetGiftCardDenomination");
    }

    //Save Gift Card
    this.SaveConsumerGiftCard = function (giftCard) {
        var response = $http({
            method: "post",
            url: "/Consumer/SaveConsumerGiftCard",
            data: JSON.stringify(giftCard),

            dataType: "json"
        });
        return response;
    }

    this.PayConsumerGiftCard = function (merchantid, giftCardId, qtyDenom1, qty, grandtotal) {
        $http.post("/Consumer/PayForGiftCards", { merchantid: merchantid, giftCardId: giftCardId, qtyDenom1: qtyDenom1, qty: qty, grandtotal: grandtotal });

        //var data = $.param({
        //    merchantid: merchantid,
        //    giftCardId: giftCardId,
        //    qtyDenom1: qtyDenom1,
        //    qty: qty,
        //    grandtotal:grandtotal
        //});

        //var config = {
        //    headers: {
        //        'Content-Type': 'application/x-www-form-urlencoded;charset=utf-8;'
        //    }
        //}

        //$http.post('/Consumser/PayForGiftCards', data, config)
        //.success(function (data, status, headers, config) {
        //    debugger;
        //    $scope.PostDataResponse = data;
        //})
        //.error(function (data, status, header, config) {
        //    $scope.ResponseDetails = "Data: " + data +
        //        "<hr />status: " + status +
        //        "<hr />headers: " + header +
        //        "<hr />config: " + config;
        //});

    }

    //Add dec 
    this.SendGCConsumer = function (mobileno, Id, MerchantId) {

        var response = $http({
            method: "post",
            url: "/Consumer/SendGCToConsumer",
            data: {
                mobileno: mobileno,
                Id: Id,
                MerchantId: MerchantId
            }
        });
        return response;
    }


});