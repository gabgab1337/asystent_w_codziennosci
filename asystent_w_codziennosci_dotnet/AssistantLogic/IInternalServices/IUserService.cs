using AssistantDatabase.Model;
using AssistantLogic.Model;
using AssistantLogic.ViewModel;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AssistantLogic.IInternalServices
{
    public interface IUserService
    {
        List<UserBaseInformationVM> ReadBaseInforOfUsersForAdmin(int id);
        List<AsdUserBaseInformationVM> ReadBaseInfoOfAsdPersonForCaregiver(int caregiver, int? selectProtageId);
        UserSessionModel? LogIn(UserLoginVM model);
        void CreateUserForAdmin(UserAddVM vm);
        void CreateUserForCaregiver(UserAddVM vm, int caregiverId);
        UserSessionModel SelectASDPerson(UserSessionModel user, int id);
        UserAddVM GetUserAddVM(UserType executorType);
        UserEditVM GetUserEditForAdminVM(int userId, UserType executorType, int actualUserId);
        UserEditVM GetUserEditForCaregiverVM(int userId, UserType executorType);
        void Update(UserEditVM model);
        void Delete(int id);
        UserType GetUserType(int userId);
        bool FillUser(UserAddVM vm, UserType executorType);
        bool FillExistUser(UserEditVM vm, UserType executorType);
        List<SelectListItem> GetUserTypesView(UserType userType);
        void SetColorsPalete(int userId, string colorPaleteName);
    }
}
