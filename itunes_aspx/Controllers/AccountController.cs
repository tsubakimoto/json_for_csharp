using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using itunes_aspx.Filters;
using itunes_aspx.Models;

namespace itunes_aspx.Controllers
{
    [Authorize]
    [InitializeSimpleMembership]
    public class AccountController : Controller
    {
        //
        // POST: /Account/JsonLogin

        [AllowAnonymous]
        [HttpPost]
        public JsonResult JsonLogin(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
                {
                    FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                    return Json(new { success = true, redirect = returnUrl });
                }
                else
                {
                    ModelState.AddModelError("", "指定されたユーザー名またはパスワードが正しくありません。");
                }
            }

            // ここまで移動した場合は、何らかのエラーが発生しています
            return Json(new { errors = GetErrorsFromModelState() });
        }

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();

            return RedirectToAction("Index", "Home");
        }

        //
        // POST: /Account/JsonRegister
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult JsonRegister(RegisterModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                // ユーザーの登録を試みます
                try
                {
                    WebSecurity.CreateUserAndAccount(model.UserName, model.Password);
                    WebSecurity.Login(model.UserName, model.Password);

                    InitiateDatabaseForNewUser(model.UserName);

                    FormsAuthentication.SetAuthCookie(model.UserName, createPersistentCookie: false);
                    return Json(new { success = true, redirect = returnUrl });
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
            }

            // ここまで移動した場合は、何らかのエラーが発生しています
            return Json(new { errors = GetErrorsFromModelState() });
        }

        /// <summary>
        /// 新しいユーザーの新しい Todo リストを開始します
        /// </summary>
        /// <param name="userName"></param>
        private static void InitiateDatabaseForNewUser(string userName)
        {
            TodoItemContext db = new TodoItemContext();
            TodoList todoList = new TodoList();
            todoList.UserId = userName;
            todoList.Title = "マイ Todo リスト #1";
            todoList.Todos = new List<TodoItem>();
            db.TodoLists.Add(todoList);
            db.SaveChanges();

            todoList.Todos.Add(new TodoItem() { Title = "Todo 項目 #1", TodoListId = todoList.TodoListId, IsDone = false });
            todoList.Todos.Add(new TodoItem() { Title = "Todo 項目 #2", TodoListId = todoList.TodoListId, IsDone = false });
            db.SaveChanges();
        }

        //
        // POST: /Account/Disassociate

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Disassociate(string provider, string providerUserId)
        {
            string ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId);
            ManageMessageId? message = null;

            // 現在ログインしているユーザーが所有者の場合にのみ、アカウントの関連付けを解除します
            if (ownerAccount == User.Identity.Name)
            {
                // トランザクションを使用して、ユーザーが最後のログイン資格情報を削除しないようにします
                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
                {
                    bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
                    if (hasLocalAccount || OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name).Count > 1)
                    {
                        OAuthWebSecurity.DeleteAccount(provider, providerUserId);
                        scope.Complete();
                        message = ManageMessageId.RemoveLoginSuccess;
                    }
                }
            }

            return RedirectToAction("Manage", new { Message = message });
        }

        //
        // GET: /Account/Manage

        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "パスワードが変更されました。"
                : message == ManageMessageId.SetPasswordSuccess ? "パスワードが設定されました。"
                : message == ManageMessageId.RemoveLoginSuccess ? "外部ログインが削除されました。"
                : "";
            ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(LocalPasswordModel model)
        {
            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.HasLocalPassword = hasLocalAccount;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasLocalAccount)
            {
                if (ModelState.IsValid)
                {
                    // 特定のエラー シナリオでは、ChangePassword は false を返す代わりに例外をスローします。
                    bool changePasswordSucceeded;
                    try
                    {
                        changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                    }
                    catch (Exception)
                    {
                        changePasswordSucceeded = false;
                    }

                    if (changePasswordSucceeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        ModelState.AddModelError("", "現在のパスワードが正しくないか、新しいパスワードが無効です。");
                    }
                }
            }
            else
            {
                // ユーザーにローカル パスワードがないため、
                // OldPassword フィールドがないことに原因があるすべての検証エラーを削除します
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        WebSecurity.CreateAccount(User.Identity.Name, model.NewPassword);
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError("", e);
                    }
                }
            }

            // ここで問題が発生した場合はフォームを再表示します
            return View(model);
        }

        //
        // POST: /Account/ExternalLogin

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback

        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
            if (!result.IsSuccessful)
            {
                return RedirectToAction("ExternalLoginFailure");
            }

            if (OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, createPersistentCookie: false))
            {
                return RedirectToLocal(returnUrl);
            }

            if (User.Identity.IsAuthenticated)
            {
                // 現在のユーザーがログインしている場合、新しいアカウントを追加します
                OAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId, User.Identity.Name);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // ユーザーは新規なので、希望するメンバーシップ名を要求します
                string loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);
                ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
                ViewBag.ReturnUrl = returnUrl;
                return View("ExternalLoginConfirmation", new RegisterExternalLoginModel { UserName = result.UserName, ExternalLoginData = loginData });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLoginConfirmation(RegisterExternalLoginModel model, string returnUrl)
        {
            string provider = null;
            string providerUserId = null;

            if (User.Identity.IsAuthenticated || !OAuthWebSecurity.TryDeserializeProviderUserId(model.ExternalLoginData, out provider, out providerUserId))
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // データベースに新しいユーザーを挿入します
                using (UsersContext db = new UsersContext())
                {
                    UserProfile user = db.UserProfiles.FirstOrDefault(u => u.UserName.ToLower() == model.UserName.ToLower());
                    // ユーザーが既に存在するかどうかを確認します
                    if (user == null)
                    {
                        // プロファイル テーブルに名前を挿入します
                        db.UserProfiles.Add(new UserProfile { UserName = model.UserName });
                        db.SaveChanges();

                        InitiateDatabaseForNewUser(model.UserName);

                        OAuthWebSecurity.CreateOrUpdateAccount(provider, providerUserId, model.UserName);
                        OAuthWebSecurity.Login(provider, providerUserId, createPersistentCookie: false);

                        return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        ModelState.AddModelError("UserName", "このユーザー名は既に存在します。別のユーザー名を入力してください。");
                    }
                }
            }

            ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(provider).DisplayName;
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // GET: /Account/ExternalLoginFailure

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ExternalLoginsList(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_ExternalLoginsListPartial", OAuthWebSecurity.RegisteredClientData);
        }

        [ChildActionOnly]
        public ActionResult RemoveExternalLogins()
        {
            ICollection<OAuthAccount> accounts = OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name);
            List<ExternalLogin> externalLogins = new List<ExternalLogin>();
            foreach (OAuthAccount account in accounts)
            {
                AuthenticationClientData clientData = OAuthWebSecurity.GetOAuthClientData(account.Provider);

                externalLogins.Add(new ExternalLogin
                {
                    Provider = account.Provider,
                    ProviderDisplayName = clientData.DisplayName,
                    ProviderUserId = account.ProviderUserId,
                });
            }

            ViewBag.ShowRemoveButton = externalLogins.Count > 1 || OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            return PartialView("_RemoveExternalLoginsPartial", externalLogins);
        }

        #region ヘルパー
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }

        private IEnumerable<string> GetErrorsFromModelState()
        {
            return ModelState.SelectMany(x => x.Value.Errors.Select(error => error.ErrorMessage));
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // すべてのステータス コードの一覧については、http://go.microsoft.com/fwlink/?LinkID=177550 を
            // 参照してください。
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "このユーザー名は既に存在します。別のユーザー名を入力してください。";

                case MembershipCreateStatus.DuplicateEmail:
                    return "その電子メール アドレスのユーザー名は既に存在します。別の電子メール アドレスを入力してください。";

                case MembershipCreateStatus.InvalidPassword:
                    return "指定されたパスワードは無効です。有効なパスワードの値を入力してください。";

                case MembershipCreateStatus.InvalidEmail:
                    return "指定された電子メール アドレスは無効です。値を確認してやり直してください。";

                case MembershipCreateStatus.InvalidAnswer:
                    return "パスワードの回復用に指定された回答が無効です。値を確認してやり直してください。";

                case MembershipCreateStatus.InvalidQuestion:
                    return "パスワードの回復用に指定された質問が無効です。値を確認してやり直してください。";

                case MembershipCreateStatus.InvalidUserName:
                    return "指定されたユーザー名は無効です。値を確認してやり直してください。";

                case MembershipCreateStatus.ProviderError:
                    return "認証プロバイダーからエラーが返されました。入力を確認してやり直してください。問題が解決しない場合は、システム管理者に連絡してください。";

                case MembershipCreateStatus.UserRejected:
                    return "ユーザーの作成要求が取り消されました。入力を確認してやり直してください。問題が解決しない場合は、システム管理者に連絡してください。";

                default:
                    return "不明なエラーが発生しました。入力を確認してやり直してください。問題が解決しない場合は、システム管理者に連絡してください。";
            }
        }
        #endregion
    }
}