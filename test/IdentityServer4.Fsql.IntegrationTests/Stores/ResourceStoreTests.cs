using IdentityModel;
using IdentityServer4.Fsql.Storage.Mappers;
using IdentityServer4.Fsql.Storage.Stores;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.Fsql.IntegrationTests.Stores
{
    public class ResourceStoreTests
    {
        private static IdentityResource CreateIdentityTestResource()
        {
            return new IdentityResource()
            {
                Name = Guid.NewGuid().ToString(),
                DisplayName = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                ShowInDiscoveryDocument = true,
                UserClaims =
                {
                    JwtClaimTypes.Subject,
                    JwtClaimTypes.Name,
                }
            };
        }

        private static ApiResource CreateApiResourceTestResource()
        {
            return new ApiResource()
            {
                Name = Guid.NewGuid().ToString(),
                ApiSecrets = new List<Secret> { new Secret("secret".ToSha256()) },
                Scopes = { Guid.NewGuid().ToString() },
                UserClaims =
                {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                }
            };
        }

        private static ApiScope CreateApiScopeTestResource()
        {
            return new ApiScope()
            {
                Name = Guid.NewGuid().ToString(),
                UserClaims =
                {
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                }
            };
        }


        [Fact]
        public async Task FindApiResourcesByNameAsync_WhenResourceExists_ExpectResourceAndCollectionsReturned()
        {
            var resource = CreateApiResourceTestResource();

            var repo = g.configurationDb.GetRepository<Storage.Entities.ApiResource>();
            var entity = resource.ToEntity();
            repo.Insert(entity);
            repo.SaveMany(entity, "UserClaims");
            repo.SaveMany(entity, "Scopes");
            repo.SaveMany(entity, "Secrets");
            repo.SaveMany(entity, "Properties");

            ApiResource foundResource;

            var store = new ResourceStore(g.configurationDb, FakeLogger<ResourceStore>.Create());
            foundResource = (await store.FindApiResourcesByNameAsync(new[] { resource.Name })).SingleOrDefault();


            Assert.NotNull(foundResource);
            Assert.True(foundResource.Name == resource.Name);

            Assert.NotNull(foundResource.UserClaims);
            Assert.NotEmpty(foundResource.UserClaims);
            Assert.NotNull(foundResource.ApiSecrets);
            Assert.NotEmpty(foundResource.ApiSecrets);
            Assert.NotNull(foundResource.Scopes);
            Assert.NotEmpty(foundResource.Scopes);
        }

        [Fact]
        public async Task FindApiResourcesByNameAsync_WhenResourcesExist_ExpectOnlyResourcesRequestedReturned()
        {
            var resource = CreateApiResourceTestResource();
            var repo = g.configurationDb.GetRepository<Storage.Entities.ApiResource>();
            var entity1 = resource.ToEntity();
            repo.Insert(entity1);
            repo.SaveMany(entity1, "UserClaims");
            repo.SaveMany(entity1, "Scopes");
            repo.SaveMany(entity1, "Secrets");
            repo.SaveMany(entity1, "Properties");
            var entity2 = CreateApiResourceTestResource().ToEntity();
            repo.Insert(entity2);
            repo.SaveMany(entity2, "UserClaims");
            repo.SaveMany(entity2, "Scopes");
            repo.SaveMany(entity2, "Secrets");
            repo.SaveMany(entity2, "Properties");

            ApiResource foundResource;
            var store = new ResourceStore(g.configurationDb, FakeLogger<ResourceStore>.Create());
            foundResource = (await store.FindApiResourcesByNameAsync(new[] { resource.Name })).SingleOrDefault();

            Assert.NotNull(foundResource);
            Assert.True(foundResource.Name == resource.Name);

            Assert.NotNull(foundResource.UserClaims);
            Assert.NotEmpty(foundResource.UserClaims);
            Assert.NotNull(foundResource.ApiSecrets);
            Assert.NotEmpty(foundResource.ApiSecrets);
            Assert.NotNull(foundResource.Scopes);
            Assert.NotEmpty(foundResource.Scopes);
        }




        [Fact]
        public async Task FindApiResourcesByScopeNameAsync_WhenResourcesExist_ExpectResourcesReturned()
        {
            var testApiResource = CreateApiResourceTestResource();
            var testApiScope = CreateApiScopeTestResource();
            testApiResource.Scopes.Add(testApiScope.Name);

            var repo = g.configurationDb.GetRepository<Storage.Entities.ApiResource>();

            var entity = testApiResource.ToEntity();
            repo.Insert(entity);
            repo.SaveMany(entity, "Scopes");

            g.configurationDb.Insert(testApiScope.ToEntity()).ExecuteAffrows();

            IEnumerable<ApiResource> resources;

            var store = new ResourceStore(g.configurationDb, FakeLogger<ResourceStore>.Create());
            resources = await store.FindApiResourcesByScopeNameAsync(new List<string>
            {
                    testApiScope.Name
            });


            Assert.NotNull(resources);
            Assert.NotEmpty(resources);
            Assert.NotNull(resources.Single(x => x.Name == testApiResource.Name));
        }


    }
}
