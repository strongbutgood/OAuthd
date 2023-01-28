using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProcessMfg.Model;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator
{
	class RolesOverview
	{
		public NonEntitySet<UserRole> Roles { get; set; }

		void Add() { }
	}

	class RoleDetail
	{
		public UserRole Role { get; set; }

		void Delete() { }

		void PermissionsTab() { }
	}
}
