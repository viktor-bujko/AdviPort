using System;
using System.Collections.Generic;
using System.Text;

namespace AdviPort {
	class Session {
		private static Session active;
		private UserProfile loggedUser;
		private DateTime loginDate;

		public static Session ActiveSession {
			get {
				if (active is null) {
					active = new Session();
				}

				return active;
			}
		}

		public UserProfile LoggedUser {
			get => loggedUser; 
			set {
				loggedUser = value;
				loginDate = DateTime.Now;
			}
		}

		private Session() {
			LoggedUser = null;
		}
	}
}
