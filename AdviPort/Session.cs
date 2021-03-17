using System;
using System.Collections.Generic;
using System.Text;

namespace AdviPort {
	class Session {
		private static Session active;
		public static Session ActiveSession {
			get {
				if (active is null) {
					active = new Session();
				}

				return active;
			}
		}



		private Session() { }
	}
}
