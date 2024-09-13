using chattingApp.vModels;

namespace chattingApp.Services
{
    public interface IAuthService
    {
        string getUserId();
        // for registering user
        Task<string> sendOtpToConfirmPhoneAsync(sendOTPForLoginModel model);
        Task<registerResult> registerAsync(userDataModel model);
        // log in
        Task<string> sendOTPToLoginAsync(sendOTPForLoginModel model);
        Task<registerResult> getTokenAsync(loginModel model);
        // log out
        Task<string> LogOutAsync();

        // Token blacklisting methods
        void BlacklistToken(string token);
        bool IsTokenBlacklisted(string token);

    }
}
