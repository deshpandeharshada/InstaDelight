﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace InstaDelight.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class instadelightEntities : DbContext
    {
        public instadelightEntities()
            : base("name=instadelightEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<admin> admins { get; set; }
        public virtual DbSet<bank_dec_details> bank_dec_details { get; set; }
        public virtual DbSet<bank_master> bank_master { get; set; }
        public virtual DbSet<bankconsumerdetail> bankconsumerdetails { get; set; }
        public virtual DbSet<branch> branches { get; set; }
        public virtual DbSet<branch_master> branch_master { get; set; }
        public virtual DbSet<brand_master> brand_master { get; set; }
        public virtual DbSet<business_category_master> business_category_master { get; set; }
        public virtual DbSet<cashbackdetail> cashbackdetails { get; set; }
        public virtual DbSet<cheque> cheques { get; set; }
        public virtual DbSet<city_master> city_master { get; set; }
        public virtual DbSet<comment> comments { get; set; }
        public virtual DbSet<consumergroup> consumergroups { get; set; }
        public virtual DbSet<corder> corders { get; set; }
        public virtual DbSet<country_master> country_master { get; set; }
        public virtual DbSet<coupon_redeem_details> coupon_redeem_details { get; set; }
        public virtual DbSet<couponcondition> couponconditions { get; set; }
        public virtual DbSet<coupons_master> coupons_master { get; set; }
        public virtual DbSet<coupontemplate> coupontemplates { get; set; }
        public virtual DbSet<currency> currencies { get; set; }
        public virtual DbSet<customer> customers { get; set; }
        public virtual DbSet<employee> employees { get; set; }
        public virtual DbSet<enquiry> enquiries { get; set; }
        public virtual DbSet<eventcouponcondition> eventcouponconditions { get; set; }
        public virtual DbSet<eventcoupondetail> eventcoupondetails { get; set; }
        public virtual DbSet<eventcoupontemplate> eventcoupontemplates { get; set; }
        public virtual DbSet<eventmaster> eventmasters { get; set; }
        public virtual DbSet<giftcardconditionsmapping> giftcardconditionsmappings { get; set; }
        public virtual DbSet<giftcardmaster> giftcardmasters { get; set; }
        public virtual DbSet<help> helps { get; set; }
        public virtual DbSet<language_master> language_master { get; set; }
        public virtual DbSet<location_master> location_master { get; set; }
        public virtual DbSet<menu> menus { get; set; }
        public virtual DbSet<menu_csv_template> menu_csv_template { get; set; }
        public virtual DbSet<merchant_master> merchant_master { get; set; }
        public virtual DbSet<merchantconsumercoupondetail> merchantconsumercoupondetails { get; set; }
        public virtual DbSet<merchantconsumerdetail> merchantconsumerdetails { get; set; }
        public virtual DbSet<merchantconsumerreviewdetail> merchantconsumerreviewdetails { get; set; }
        public virtual DbSet<merchantjoiningbonu> merchantjoiningbonus { get; set; }
        public virtual DbSet<merchantreviewcomment> merchantreviewcomments { get; set; }
        public virtual DbSet<merchantsalesreportlog> merchantsalesreportlogs { get; set; }
        public virtual DbSet<merchantsalesutilitymaster> merchantsalesutilitymasters { get; set; }
        public virtual DbSet<merchantsmsdetail> merchantsmsdetails { get; set; }
        public virtual DbSet<package> packages { get; set; }
        public virtual DbSet<proposal> proposals { get; set; }
        public virtual DbSet<redeemmaster> redeemmasters { get; set; }
        public virtual DbSet<redeemoption> redeemoptions { get; set; }
        public virtual DbSet<report> reports { get; set; }
        public virtual DbSet<rest_registration> rest_registration { get; set; }
        public virtual DbSet<review_rating_star> review_rating_star { get; set; }
        public virtual DbSet<review_submit_details> review_submit_details { get; set; }
        public virtual DbSet<reviewmaster> reviewmasters { get; set; }
        public virtual DbSet<reviewtemplate> reviewtemplates { get; set; }
        public virtual DbSet<rewardmaster> rewardmasters { get; set; }
        public virtual DbSet<role> roles { get; set; }
        public virtual DbSet<sale> sales { get; set; }
        public virtual DbSet<staff_master> staff_master { get; set; }
        public virtual DbSet<state_master> state_master { get; set; }
        public virtual DbSet<ucomment> ucomments { get; set; }
        public virtual DbSet<userclaim> userclaims { get; set; }
        public virtual DbSet<userlogin> userlogins { get; set; }
        public virtual DbSet<userrole> userroles { get; set; }
        public virtual DbSet<varcode> varcodes { get; set; }
        public virtual DbSet<user> users { get; set; }
        public virtual DbSet<bank_benefits> bank_benefits { get; set; }
        public virtual DbSet<merchant_benefits> merchant_benefits { get; set; }
    
        public virtual ObjectResult<GetSubmittedReviewDetails_Result> GetSubmittedReviewDetails(Nullable<int> rid, Nullable<int> mid)
        {
            var ridParameter = rid.HasValue ?
                new ObjectParameter("rid", rid) :
                new ObjectParameter("rid", typeof(int));
    
            var midParameter = mid.HasValue ?
                new ObjectParameter("mid", mid) :
                new ObjectParameter("mid", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetSubmittedReviewDetails_Result>("GetSubmittedReviewDetails", ridParameter, midParameter);
        }
    
        public virtual ObjectResult<GetBankListForConsumers_Result> GetBankListForConsumers(string cId)
        {
            var cIdParameter = cId != null ?
                new ObjectParameter("CId", cId) :
                new ObjectParameter("CId", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetBankListForConsumers_Result>("GetBankListForConsumers", cIdParameter);
        }
    
        public virtual ObjectResult<GetCouponListForAllMerchants_Result> GetCouponListForAllMerchants(string merchantCity, Nullable<int> merchantCategory, string cId)
        {
            var merchantCityParameter = merchantCity != null ?
                new ObjectParameter("MerchantCity", merchantCity) :
                new ObjectParameter("MerchantCity", typeof(string));
    
            var merchantCategoryParameter = merchantCategory.HasValue ?
                new ObjectParameter("MerchantCategory", merchantCategory) :
                new ObjectParameter("MerchantCategory", typeof(int));
    
            var cIdParameter = cId != null ?
                new ObjectParameter("CId", cId) :
                new ObjectParameter("CId", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetCouponListForAllMerchants_Result>("GetCouponListForAllMerchants", merchantCityParameter, merchantCategoryParameter, cIdParameter);
        }
    
        public virtual ObjectResult<GetCouponListForMerchant_Result> GetCouponListForMerchant(string mId, string cId)
        {
            var mIdParameter = mId != null ?
                new ObjectParameter("MId", mId) :
                new ObjectParameter("MId", typeof(string));
    
            var cIdParameter = cId != null ?
                new ObjectParameter("CId", cId) :
                new ObjectParameter("CId", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetCouponListForMerchant_Result>("GetCouponListForMerchant", mIdParameter, cIdParameter);
        }
    
        public virtual ObjectResult<GetMerchantCountryDetails_Result> GetMerchantCountryDetails(string mId)
        {
            var mIdParameter = mId != null ?
                new ObjectParameter("MId", mId) :
                new ObjectParameter("MId", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetMerchantCountryDetails_Result>("GetMerchantCountryDetails", mIdParameter);
        }
    
        public virtual ObjectResult<GetMerchantListForConsumer_Result> GetMerchantListForConsumer(string cId)
        {
            var cIdParameter = cId != null ?
                new ObjectParameter("CId", cId) :
                new ObjectParameter("CId", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetMerchantListForConsumer_Result>("GetMerchantListForConsumer", cIdParameter);
        }
    
        public virtual ObjectResult<GetMerchantListForPendingReviews_Result> GetMerchantListForPendingReviews(string cId)
        {
            var cIdParameter = cId != null ?
                new ObjectParameter("CId", cId) :
                new ObjectParameter("CId", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetMerchantListForPendingReviews_Result>("GetMerchantListForPendingReviews", cIdParameter);
        }
    
        public virtual ObjectResult<GetReviewDetailsByMerchantId_Result> GetReviewDetailsByMerchantId(string mId, string cId)
        {
            var mIdParameter = mId != null ?
                new ObjectParameter("MId", mId) :
                new ObjectParameter("MId", typeof(string));
    
            var cIdParameter = cId != null ?
                new ObjectParameter("CId", cId) :
                new ObjectParameter("CId", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetReviewDetailsByMerchantId_Result>("GetReviewDetailsByMerchantId", mIdParameter, cIdParameter);
        }
    
        public virtual int InsertConsumerCouponDetails(string merchantId, Nullable<int> couponId, string consumerId, string consumerPhone, Nullable<System.DateTime> validFrom, Nullable<System.DateTime> validTo)
        {
            var merchantIdParameter = merchantId != null ?
                new ObjectParameter("MerchantId", merchantId) :
                new ObjectParameter("MerchantId", typeof(string));
    
            var couponIdParameter = couponId.HasValue ?
                new ObjectParameter("CouponId", couponId) :
                new ObjectParameter("CouponId", typeof(int));
    
            var consumerIdParameter = consumerId != null ?
                new ObjectParameter("ConsumerId", consumerId) :
                new ObjectParameter("ConsumerId", typeof(string));
    
            var consumerPhoneParameter = consumerPhone != null ?
                new ObjectParameter("ConsumerPhone", consumerPhone) :
                new ObjectParameter("ConsumerPhone", typeof(string));
    
            var validFromParameter = validFrom.HasValue ?
                new ObjectParameter("ValidFrom", validFrom) :
                new ObjectParameter("ValidFrom", typeof(System.DateTime));
    
            var validToParameter = validTo.HasValue ?
                new ObjectParameter("ValidTo", validTo) :
                new ObjectParameter("ValidTo", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("InsertConsumerCouponDetails", merchantIdParameter, couponIdParameter, consumerIdParameter, consumerPhoneParameter, validFromParameter, validToParameter);
        }
    
        public virtual int InsertMerchantConsumerDetails(string merchantId, string consumerId, string consumerPhone)
        {
            var merchantIdParameter = merchantId != null ?
                new ObjectParameter("MerchantId", merchantId) :
                new ObjectParameter("MerchantId", typeof(string));
    
            var consumerIdParameter = consumerId != null ?
                new ObjectParameter("ConsumerId", consumerId) :
                new ObjectParameter("ConsumerId", typeof(string));
    
            var consumerPhoneParameter = consumerPhone != null ?
                new ObjectParameter("ConsumerPhone", consumerPhone) :
                new ObjectParameter("ConsumerPhone", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("InsertMerchantConsumerDetails", merchantIdParameter, consumerIdParameter, consumerPhoneParameter);
        }
    
        public virtual int InsertMerchantReviewDetails(string merchantId, string consumerId, Nullable<int> reviewId, string reviewStatus, Nullable<System.DateTime> sharedDate, Nullable<System.DateTime> validTill)
        {
            var merchantIdParameter = merchantId != null ?
                new ObjectParameter("MerchantId", merchantId) :
                new ObjectParameter("MerchantId", typeof(string));
    
            var consumerIdParameter = consumerId != null ?
                new ObjectParameter("ConsumerId", consumerId) :
                new ObjectParameter("ConsumerId", typeof(string));
    
            var reviewIdParameter = reviewId.HasValue ?
                new ObjectParameter("ReviewId", reviewId) :
                new ObjectParameter("ReviewId", typeof(int));
    
            var reviewStatusParameter = reviewStatus != null ?
                new ObjectParameter("ReviewStatus", reviewStatus) :
                new ObjectParameter("ReviewStatus", typeof(string));
    
            var sharedDateParameter = sharedDate.HasValue ?
                new ObjectParameter("SharedDate", sharedDate) :
                new ObjectParameter("SharedDate", typeof(System.DateTime));
    
            var validTillParameter = validTill.HasValue ?
                new ObjectParameter("ValidTill", validTill) :
                new ObjectParameter("ValidTill", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("InsertMerchantReviewDetails", merchantIdParameter, consumerIdParameter, reviewIdParameter, reviewStatusParameter, sharedDateParameter, validTillParameter);
        }
    
        public virtual ObjectResult<TestGetCouponListForAllMerchants_Result> TestGetCouponListForAllMerchants(string merchantCity, Nullable<int> merchantCategory, string consumerId)
        {
            var merchantCityParameter = merchantCity != null ?
                new ObjectParameter("MerchantCity", merchantCity) :
                new ObjectParameter("MerchantCity", typeof(string));
    
            var merchantCategoryParameter = merchantCategory.HasValue ?
                new ObjectParameter("MerchantCategory", merchantCategory) :
                new ObjectParameter("MerchantCategory", typeof(int));
    
            var consumerIdParameter = consumerId != null ?
                new ObjectParameter("ConsumerId", consumerId) :
                new ObjectParameter("ConsumerId", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<TestGetCouponListForAllMerchants_Result>("TestGetCouponListForAllMerchants", merchantCityParameter, merchantCategoryParameter, consumerIdParameter);
        }
    
        public virtual ObjectResult<GetBanksMerchantsByConsumerId_Result> GetBanksMerchantsByConsumerId(string cId)
        {
            var cIdParameter = cId != null ?
                new ObjectParameter("CId", cId) :
                new ObjectParameter("CId", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetBanksMerchantsByConsumerId_Result>("GetBanksMerchantsByConsumerId", cIdParameter);
        }
    
        public virtual ObjectResult<getSupportMembers_Result> getSupportMembers(string vARCodeName, string roleName)
        {
            var vARCodeNameParameter = vARCodeName != null ?
                new ObjectParameter("VARCodeName", vARCodeName) :
                new ObjectParameter("VARCodeName", typeof(string));
    
            var roleNameParameter = roleName != null ?
                new ObjectParameter("RoleName", roleName) :
                new ObjectParameter("RoleName", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<getSupportMembers_Result>("getSupportMembers", vARCodeNameParameter, roleNameParameter);
        }
    
        public virtual ObjectResult<GetCouponListForBrand_Result> GetCouponListForBrand(string mId, string cId, string sId)
        {
            var mIdParameter = mId != null ?
                new ObjectParameter("MId", mId) :
                new ObjectParameter("MId", typeof(string));
    
            var cIdParameter = cId != null ?
                new ObjectParameter("CId", cId) :
                new ObjectParameter("CId", typeof(string));
    
            var sIdParameter = sId != null ?
                new ObjectParameter("SId", sId) :
                new ObjectParameter("SId", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetCouponListForBrand_Result>("GetCouponListForBrand", mIdParameter, cIdParameter, sIdParameter);
        }
    
        public virtual ObjectResult<GetCouponListForStaff_Result> GetCouponListForStaff(string mId, string cId, string sId, string sLoc)
        {
            var mIdParameter = mId != null ?
                new ObjectParameter("MId", mId) :
                new ObjectParameter("MId", typeof(string));
    
            var cIdParameter = cId != null ?
                new ObjectParameter("CId", cId) :
                new ObjectParameter("CId", typeof(string));
    
            var sIdParameter = sId != null ?
                new ObjectParameter("SId", sId) :
                new ObjectParameter("SId", typeof(string));
    
            var sLocParameter = sLoc != null ?
                new ObjectParameter("SLoc", sLoc) :
                new ObjectParameter("SLoc", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetCouponListForStaff_Result>("GetCouponListForStaff", mIdParameter, cIdParameter, sIdParameter, sLocParameter);
        }
    }
}