using AssistantDatabase.IRepositories;
using AssistantDatabase.Model;
using AssistantLogic.Common;
using AssistantLogic.IInternalServices;
using AssistantLogic.Model;
using AssistantLogic.ViewModel;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AssistantLogic.InternalServices
{
    public class UserService : IUserService
    {
        private IUserRepository userRepository;

        public UserService(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public List<UserBaseInformationVM> ReadBaseInforOfUsersForAdmin(int id)
        {
            List<UserDM> listDTO = userRepository.ReadAllForAdmin();
            List<UserBaseInformationVM> listVM = new List<UserBaseInformationVM>();

            foreach (UserDM dto in listDTO)
            {
                UserBaseInformationVM vm = new UserBaseInformationVM();
                vm.Id = dto.Id;
                vm.Username = dto.Username;
                vm.IsActive = dto.IsActive;
                vm.Type = dto.Type;
                vm.CanDelete = vm.Id == id ? false : true;
                vm.JoiningDate = DateComponents.AppearDate(dto.JoiningDate);

                listVM.Add(vm);
            }

            return listVM;
        }
        public List<AsdUserBaseInformationVM> ReadBaseInfoOfAsdPersonForCaregiver(int caregiver, int? selectProtageId)
        {
            List<UserDM> listDTO = userRepository.ReadAllForCaregiver(caregiver);
            List<AsdUserBaseInformationVM> listVM = new List<AsdUserBaseInformationVM>();

            foreach (UserDM dto in listDTO)
            {
                AsdUserBaseInformationVM vm = new AsdUserBaseInformationVM();
                vm.Id = dto.Id;
                vm.Username = dto.Username;
                vm.IsActive = dto.IsActive;
                vm.JoiningDate = DateComponents.AppearDate(dto.JoiningDate);
                vm.IsSelected = selectProtageId != null && selectProtageId == dto.Id;
                listVM.Add(vm);
            }

            return listVM;
        }


        public UserSessionModel SelectASDPerson(UserSessionModel user, int id)
        {
            UserDM asdUser = userRepository.GetById(id);
            user.SelectedProtageName = asdUser.Username;
            user.SelectedProtageId = id;

            UserDM userDM = userRepository.GetById(user.Id);
            userDM.SelectedASD = id;
            userRepository.Update(userDM);

            return user;
        }

        public UserSessionModel? LogIn(UserLoginVM model)
        {
            UserDM? userDM = userRepository.IsLogIn(model.Username, model.Password);
            if (userDM == null)
            {
                return null;
            }
            UserSessionModel user = new UserSessionModel();
            user.Id = userDM.Id;
            user.UserName = userDM.Username;
            user.Type = userDM.Type;

            if(user.Type == UserType.caregiver)
            {
                user.SelectedProtageId = userDM.SelectedASD;
                if (userDM.SelectedASD != null)
                {
                    UserDM? protege = userRepository.GetProtegeById(user.SelectedProtageId);
                    if(protege!=null)
                    {
                        user.SelectedProtageName = protege.Username;
                    }
                    else
                    {
                        user.SelectedProtageId = null;
                    }
                    
                    
                }
            }

            if(userDM.ColorsPalete == null)
            {
                user.ColorPalete = "White";
            }
            else
            {
                user.ColorPalete = userDM.ColorsPalete;
            }

            return user;
        }
        private bool ExistLogin(string userName)
        {
            return userRepository.ExistLogin(userName);
        }
        public bool FillUser(UserAddVM vm, UserType executorType)
        {
            vm.NameExists = ExistLogin(vm.Username);
            vm.Types = GetUserTypes(executorType);
            vm.actualType = executorType;
            return vm.NameExists;
        }
        public bool FillExistUser(UserEditVM vm, UserType executorType)
        {
            vm.NameExists = ExistLogin(vm.Username);
            vm.Types = GetUserTypes(executorType);
            vm.actualType = executorType;
            return vm.NameExists;
        }
        public void CreateUserForAdmin(UserAddVM vm)
        {
            UserDM dm = new UserDM();
            dm = FromUserAddVMToUserDM(vm, dm);
            userRepository.Add(dm);
        }
        public void CreateUserForCaregiver(UserAddVM vm, int caregiverId)
        {
            UserDM dm = new UserDM();
            dm = FromUserAddVMToUserDM(vm, dm);
            dm.CaregiverId = caregiverId;
            dm.Type = UserType.asdPerson;
            userRepository.Add(dm);
        }

        public UserAddVM GetUserAddVM(UserType executorType)
        {
            UserAddVM user = ToUserAddVM(new UserDM(), executorType);
            return user;
        }
        public UserEditVM GetUserEditForAdminVM(int userId, UserType executorType, int actualUserId)
        {
            UserDM userDM = userRepository.GetById(userId);
            UserEditVM user = ToUserEditVM(userDM, executorType);
            user.CanEditType = user.Id != actualUserId;
            return user;
        }
        public UserEditVM GetUserEditForCaregiverVM(int userId, UserType executorType)
        {
            UserDM userDM = userRepository.GetById(userId);
            UserEditVM user = ToUserEditVM(userDM, executorType);
            return user;
        }

        public void Update(UserEditVM model)
        {
            UserDM user = userRepository.GetById(model.Id);
            user = FromUserEditVMToUserDM(model, user);
            userRepository.Update(user);
        }

        public void Delete(int id)
        {
            UserDM user = userRepository.GetById(id);
            if(user.Type == UserType.asdPerson)
            {
                UserDM caregiver = userRepository.GetById(user.CaregiverId.Value);
                if(caregiver.SelectedASD!= null && caregiver.SelectedASD == id)
                {
                    caregiver.SelectedASD = null;
                    userRepository.Update(caregiver);
                }
                userRepository.DeleteAsdPerson(user);
            }
            else if (user.Type == UserType.caregiver)
            {
                userRepository.DeleteCaregiver(user);
            }
            else
            {
                userRepository.DeleteAdmin(user);
            }
            
                
        }

        public UserType GetUserType(int userId)
        {
            return userRepository.GetUserType(userId);
        }

        private UserAddVM ToUserAddVM(UserDM dm, UserType executorType)
        {
            UserAddVM vm = new UserAddVM();

            vm.Id = dm.Id;
            vm.Username = dm.Username;
            vm.Type = dm.Type;
            vm.actualType = executorType;
            vm.Types = GetUserTypes(executorType);
            
            if (executorType == UserType.caregiver)
            {
                vm.Type = UserType.asdPerson;
            }

            return vm;
        }
        private UserEditVM ToUserEditVM(UserDM dm, UserType executorType)
        {
            UserEditVM vm = new UserEditVM();

            vm.Id = dm.Id;
            vm.Username = dm.Username;
            vm.UsernameBefore = vm.Username;
            vm.Type = dm.Type;
            vm.actualType = executorType;
            vm.Types = GetUserTypes(executorType);

            if (executorType == UserType.caregiver)
            {
                vm.Type = UserType.asdPerson;
            }

            return vm;
        }
        private List<SelectListItem> GetUserTypes(UserType executorType)
        {
            List<SelectListItem> types = new List<SelectListItem>();
            if (executorType == UserType.administrator)
            {
                types = UserType.administrator.GetTypesList();
                types.RemoveAt(types.Count - 1);
            }
            return types;
        }
        public List<SelectListItem> GetUserTypesView(UserType userType)
        {
            var types = new List<SelectListItem>();

            if (userType == UserType.administrator)
            {
                types.Add(new SelectListItem { Text = "Administrator", Value = UserType.administrator.ToString() });
                types.Add(new SelectListItem { Text = "Opiekun", Value = UserType.caregiver.ToString() });
            }
            else if (userType == UserType.caregiver)
            {
                types.Add(new SelectListItem { Text = "Osoba z ASD", Value = UserType.asdPerson.ToString() });
            }

            return types;
        }
        private UserDM FromUserAddVMToUserDM(UserAddVM vm, UserDM dm)
        {
            dm.Id = vm.Id;
            dm.Username = vm.Username;
            dm.Type = vm.Type;
            return dm;
        }
        private UserDM FromUserEditVMToUserDM(UserEditVM vm, UserDM dm)
        {
            dm.Id = vm.Id;
            dm.Username = vm.Username;
            dm.Type = vm.Type;
            return dm;
        }
        public void SetColorsPalete(int userId, string colorPaleteName)
        {
            userRepository.SetColorsPalete(userId, colorPaleteName);
        }
    }
}

