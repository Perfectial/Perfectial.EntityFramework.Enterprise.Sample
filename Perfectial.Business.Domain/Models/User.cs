using Perfectial.Core.Domain.Base;
using Perfectial.Core.Domain.Enums;
using Perfectial.Core.Domain.Models;
using Perfectial.Core.Domain.Models.Base;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Perfectial.Core.Domain.Model
{
    public class User : AggregateRoot<User>
    {
        #region Properties
        public string Name { get; private set; }
		public string Email { get; private set; }
		public int CreditScore { get; private set; }
        public string Password { get; private set; }
		public bool WelcomeEmailSent { get; private set; }
		public DateTime CreatedOn { get; private set; }
        public UserType UserType { get; private set; }
        #endregion

        #region Relations
        public Role Role { get; set; }
        #endregion

        #region Actions
        public override string ToString()
		{
			return String.Format("Id: {0} | Name: {1} | Email: {2} | CreditScore: {3} | WelcomeEmailSent: {4} | CreatedOn (UTC): {5}", Id, Name, Email, CreditScore, WelcomeEmailSent, CreatedOn.ToString("dd MMM yyyy - HH:mm:ss"));
        }

        public void Delete()
        {
            Deleted = true;
        }

        public bool Validate()
        {
            return true;
        }

        #endregion

        //TODO: maybe remove queries specification from domain model, use Specification pattern
        #region Root Measures
        public override List<Expression<Func<User, object>>> GetRootMeassures()
        {
            return new List<Expression<Func<User, object>>> { u => u.Role };
        }
        #endregion

        #region Repository Specifications
        public static Expression<Func<User, bool>> MostScoredUserSpecification()
        {
            return u => u.CreditScore > 200;
        }
        #endregion
    }
}
