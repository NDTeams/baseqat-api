using System;
using System.Collections.Generic;
using System.Text;

namespace Baseqat.EF.Consts
{
    public static class ResponseMessages
    {
        public const string DataRetrieved = "تم استرجاع البيانات بنجاح";
        public const string DataSaved = "تم الحفظ بنجاح";
        public const string DataUpdated = "تم التحديث بنجاح";
        public const string DataDeleted = "تم الحذف بنجاح";
        public const string NotFound = "البيانات غير موجودة";
        public const string UnknownError = "حدث خطأ غير متوقع، الرجاء المحاولة لاحقًا";
        public const string InvalidCredentials = "بيانات الدخول غير صحيحة";
        public const string UserAlreadyExists = "المستخدم موجود بالفعل";
        public const string EmailAlreadyConfirmed = "البريد الإلكتروني تم تأكيده مسبقًا";
        public const string EmailConfirmationSent = "تم إرسال رابط تأكيد البريد الإلكتروني";
        public const string EmailConfirmed = "تم تأكيد البريد الإلكتروني بنجاح";
        public const string PasswordResetSent = "تم إرسال رابط إعادة تعيين كلمة المرور";
        public const string PasswordResetSuccess = "تم إعادة تعيين كلمة المرور بنجاح";
        public const string PasswordResetFailed = "فشل في إعادة تعيين كلمة المرور";
        public const string AccessDenied = "ليس لديك صلاحية للوصول";
        public const string OperationFailed = "فشلت العملية، الرجاء المحاولة لاحقًا";
        public const string Enable2FAFailed = "فشل في تفعيل التحقق بخطوتين";
        public const string Disable2FAFailed = "فشل في إيقاف التحقق بخطوتين";
        public const string TwoFactorEnabled = "تم تفعيل التحقق بخطوتين بنجاح";
        public const string TwoFactorDisabled = "تم إيقاف التحقق بخطوتين بنجاح";
        public const string Invalid2FACode = "رمز التحقق بخطوتين غير صحيح";
        public const string RoleNotFound = "الدور غير موجود";
        public const string UserNotFound = "المستخدم غير موجود";
        public const string EmailInUse = "البريد الإلكتروني مستخدم بالفعل";
        public const string SoftDeleteFail = "لا يتم دعم الحذف غير الدائم على هذا الجدول.";
        public const string InvalidData = "البيانات المدخلة غير صحيحة";
        public const string PhoneNumberInUse = "رقم الجوال مستخدم بالفعل";
        public const string UserAddedToRole = "تم اضافة المستخدم الى المجموعة بنجاح";
        public const string OtpSendFailed = "فشل في إرسال رمز التحقق";
        public const string OtpSent = "تم إرسال رمز التحقق بنجاح";
        public const string InvalidOTP = "رمز التحقق غير صحيح";
        public const string LoginSuccessful = "تم تسجيل الدخول بنجاح";
        public const string RegistrationSuccessful = "تم التسجيل بنجاح";
        public const string OtpVerified = "تم التحقق من الرمز بنجاح";
        public const string CompanyAlreadyExistsForUser = "الشركة موجودة بالفعل لهذا المستخدم";
        public const string InvalidImageFileType = "نوع ملف الصورة غير صالح";
        public const string ImageSizeExceeded = "يجب ألا يتجاوز حجم الصورة 500 كيلوبايت.";
        public const string PhoneNumberNotConfirmed = "لم يتم تأكيد رقم الجوال";
        public const string EmailSendFailed = "فشل في إرسال البريد الإلكتروني";
        public const string EmailNotSent = "لم يتم ارسال البريد الإلكتروني";
        public const string EmailNotConfirmed = "لم يتم تأكيد البريد الإلكتروني";
        public const string InvalidOrExpiredVerificationLink = "رابط التحقق غير صالح أو انتهت صلاحيته";
        public const string FileSizeExceeded = "حجم الملف يتجاوز 2 ميجابايت.";

    }
}
