using DreamJob.Models;
using DreamJob.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace DreamJob.Controllers.Home
{
    
    public class HomeController : Controller
    {
        private Dal dal;

        // Constructeur externe
        public HomeController() : this(new Dal())
        {

        }

        // Constructeur interne
        private HomeController(Dal dalIoc)
        {
            dal = dalIoc;
        }

        // Route vers la page des offres d'emploi
        public ActionResult Index()
        {
            Dal dal = new Dal();
            HomeViewModel vm = new HomeViewModel
            {
                ListeDesArticles = dal.ObtientTousLesArticles(),
                Authentifie = HttpContext.User.Identity.IsAuthenticated
            };
            return View(vm);
        }

        // Route POST vers la vue partielle d'authentification (appelee lors d'une connexion)
        [HttpPost]
        public ActionResult Index(HomeViewModel viewModel)
        {
            // Si les donnees d'identification ont un format valide ...
            if (ModelState.IsValid)
            {
                Utilisateur utilisateur = dal.Authentifier(viewModel.Utilisateur.Prenom, viewModel.Utilisateur.MotDePasse);
                // et que l'utilisateur est dans la base de donnee
                if (utilisateur != null)
                {
                    // identifier l'utilisateur par un cookie
                    FormsAuthentication.SetAuthCookie(utilisateur.Id.ToString(), false);
                    viewModel.ListeDesArticles = dal.ObtientTousLesArticles();
                    viewModel.Authentifie = true;
                    dal.RenewAllVoteState();
                    // reactualiser la vue partielle d'indentification
                    return View(viewModel);
                }
                // Si l'utilisateur n'est pas dans la base de donnee ou que le mot de passe est incorrect, envoyer une erreur
                ModelState.AddModelError("Utilisateur.Prenom", "Prénom et/ou mot de passe incorrect(s)");
            }
            // Si le format n'est pas valide, renvoyer la ve partielle d'identification avec une erreur
            return View(viewModel);
        }

        // Route autorisee seulement aux utilisateurs authentifies
        [Authorize]
        // Route pour voter pour une offre d'emploi
        public ActionResult UpVote(int id, HomeViewModel viewModel)
        {
            Dal dal = new Dal();
            dal.Vote(id,1);
            viewModel.ListeDesArticles = dal.ObtientTousLesArticles();
            viewModel.Authentifie = true;
            return View("Index", viewModel);
        }

        // Route autorisee seulement aux utilisateurs authentifies
        [Authorize]
        // Route pour voter contre une offre d'emploi
        public ActionResult DownVote(int id, HomeViewModel viewModel)
        {
            Dal dal = new Dal();
            dal.Vote(id,-1);
            viewModel.ListeDesArticles = dal.ObtientTousLesArticles();
            viewModel.Authentifie = true;
            return View("Index", viewModel);
        }
    }
}