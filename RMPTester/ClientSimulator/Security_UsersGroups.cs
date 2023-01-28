using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProcessMfg.Model;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator
{
	/* ICEBOX: Jeph: have module user determined by interrogating the user groups and taking the S-ID of the appropriate user
	* the ServerAPI.MyDodgyPrincipal/Identity so the sub and id claims are obtained from the appropriate user
	*/
	class UsersGroupsOverview
	{
		public NonEntitySet<UserGroup> UserGroups { get; set; }

		void AddUser() { }
		void AddGroup() { }
	}

	class UsersGroupsDetail
	{
		public UserGroup UserGroup { get; set; }

		public NonEntitySet<UserRole> Roles { get; set; }
		public NonEntitySet<UserRole> GroupRoles { get; set; }
		public EntitySet<Equipment> Equipment { get; set; }
		public List<UserGroupEquipment> GroupEquipment { get; set; }
		public NonEntitySet<ParameterGroup> ParameterGroups { get; set; }
		public List<UserGroupParameterGroup> GroupParameterGroups { get; set; }

		void Delete() { }
		void SetLanguage()
		{
			// CHECK: This is only visible from User (not "Group") page
		}

		void RolesTab() { }
		void PermissionsTab() { }
		void EquipmentTab() { }
		void ParameterGroupsTab() { }
	}
}
