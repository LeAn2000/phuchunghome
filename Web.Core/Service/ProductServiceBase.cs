using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web.Core.Dto;
using Web.Core.Model;
using Web.Core.Util;

namespace Web.Core.Service
{
    public abstract class ProductServiceBase : IServiceBase<ProductDto, int>
    {
        public virtual void DeleteById(int key, string userSession = null)
        {
            using (var context = new MyContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    Product product = context.Products.FirstOrDefault(x => x.Id == key);

                    context.ProductAttributes.RemoveRange(product.ProductAttributes);
                    context.ProductImages.RemoveRange(product.ProductImages);
                    context.ProductRelateds.RemoveRange(product.ProductRelateds);
                    context.Reviews.RemoveRange(product.Reviews);
                    context.Products.Remove(product);

                    context.SaveChanges();
                    transaction.Commit();
                }
            }
        }

        public virtual List<ProductDto> GetAll()
        {
            using (var context = new MyContext())
            {
                return context.Products
                    .Select(x => new ProductDto()
                    {
                        Id = x.Id,
                        Alias = x.Alias,
                        CategoryId = x.CategoryId,
                        Created = x.Created,
                        Description = x.Description,
                        DiscountPercent = x.DiscountPercent,
                        DiscountPrice = x.DiscountPrice,
                        Selling = x.Selling,
                        Image = x.Image,
                        Index = x.Index,
                        MenuId = x.MenuId,
                        Price = x.Price,
                        Name = x.Name,
                        ShortDescription = x.ShortDescription,
                        Status = x.Status,
                        UserDef1 = x.UserDef1,
                        UserDef2 = x.UserDef2,
                        UserDef3 = x.UserDef3,
                        UserDef4 = x.UserDef4,
                        UserDef5 = x.UserDef5,
                        Tags = x.Tags,
                        Menu = x.Menu == null ? null : new MenuDto()
                        {
                            Name = x.Menu.Name
                        }
                    })
                    .ToList();
            }
        }

        public virtual List<ProductDto> GetAllSelling()
        {
            using (var context = new MyContext())
            {
                return context.Products
                    .Where(x => x.Selling == true && x.Status == 10)
                    .Select(x => new ProductDto()
                    {
                        Id = x.Id,
                        Alias = x.Alias,
                        DiscountPercent = x.DiscountPercent,
                        DiscountPrice = x.DiscountPrice,
                        Image = x.Image,
                        Price = x.Price,
                        Name = x.Name
                    })
                    .ToList();
            }
        }

        public virtual List<ProductDto> GetByMenu(int menuId, string orderBy, ref int totalRecord, int page = 1, int pageSize = 1)
        {
            using (var context = new MyContext())
            {

                var checkmenu = context.Menus.Where(x => x.ParentMenu == menuId).Select(x=>x.Id).ToList();
                List<ProductDto> query = new List<ProductDto>();

                if (checkmenu.Any())
                {

                    query = (from x in context.Products
                             where checkmenu.Contains((int)x.MenuId)
                             select new ProductDto()
                             {
                                 Id = x.Id,
                                 Alias = x.Alias,
                                 DiscountPercent = x.DiscountPercent,
                                 DiscountPrice = x.DiscountPrice,
                                 Image = x.Image,
                                 Price = x.Price,
                                 Name = x.Name
                             }).ToList();
                }
                else
                {
                    query = (from x in context.Products
                             where x.MenuId == menuId
                             select new ProductDto()
                             {
                                 Id = x.Id,
                                 Alias = x.Alias,
                                 DiscountPercent = x.DiscountPercent,
                                 DiscountPrice = x.DiscountPrice,
                                 Image = x.Image,
                                 Price = x.Price,
                                 Name = x.Name
                             }).ToList();
                }

                if (!string.IsNullOrWhiteSpace(orderBy))
                {
                    switch (orderBy)
                    {
                        case "name-asc":
                            query = query.OrderBy(x => x.Name).ToList();
                            break;
                        case "name-desc":
                            query = query.OrderByDescending(x => x.Name).ToList();
                            break;
                        case "price-asc":
                            query = query.OrderBy(x => x.Price).ToList();
                            break;
                        case "price-desc":
                            query = query.OrderByDescending(x => x.Price).ToList();
                            break;
                    }
                }
                totalRecord = query.Count();
                return query
                    .Select(x => new ProductDto()
                    {
                        Id = x.Id,
                        Alias = x.Alias,
                        DiscountPercent = x.DiscountPercent,
                        DiscountPrice = x.DiscountPrice,
                        Image = x.Image,
                        Price = x.Price,
                        Name = x.Name
                    }).OrderBy(x => x.Price).Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }
        }

        public virtual List<ProductDto> GetByMenu(int menuId, string orderBy = "")
        {
            using (var context = new MyContext())
            {
                var query = context.Products
                    .Where(x => x.Status == 10 && x.MenuId == menuId);

                if (!string.IsNullOrWhiteSpace(orderBy))
                {
                    switch (orderBy)
                    {
                        case "name-asc":
                            query = query.OrderBy(x => x.Name);
                            break;
                        case "name-desc":
                            query = query.OrderByDescending(x => x.Name);
                            break;
                        case "price-asc":
                            query = query.OrderBy(x => x.Price);
                            break;
                        case "price-desc":
                            query = query.OrderByDescending(x => x.Price);
                            break;
                    }
                }

                return query
                    .Select(x => new ProductDto()
                    {
                        Id = x.Id,
                        Alias = x.Alias,
                        DiscountPercent = x.DiscountPercent,
                        DiscountPrice = x.DiscountPrice,
                        Image = x.Image,
                        Price = x.Price,
                        Name = x.Name
                    }).ToList();
            }
        }

        public virtual List<ProductDto> Search(string keySearch, string orderBy, string Typeid, string price, ref int totalRecord, int page = 1, int pageSize = 100000)
        {
            if (string.IsNullOrWhiteSpace(keySearch))
                keySearch = null;

            using (var context = new MyContext())
            {
                //var query = context.Products
                //    .Where(x => x.Status == 10)
                //    .Where(x => keySearch == null || x.Name.Contains(keySearch));
                var query = from p in context.Products
                            join m in context.Menus on p.MenuId equals m.Id
                            //where p.Status == 10
                            select new
                            {
                                Id = p.Id,
                                Alias = p.Alias,
                                DiscountPercent = p.DiscountPercent,
                                DiscountPrice = p.DiscountPrice,
                                Image = p.Image,
                                Price = p.Price,
                                Name = p.Name,
                                ParentName = m.Name
                            };

                if (!string.IsNullOrEmpty(keySearch))
                {
                    var checkproduct = context.ProductAttributes.Where(x => x.ValueString.Contains(keySearch)).Select(x=>x.ProductId).Distinct();
                    if (checkproduct.Any())
                    {
                        //var query1 = from p in context.Products
                        //            join m in context.Menus on p.MenuId equals m.Id
                        //            join md in checkproduct on p.Id equals md
                        //            select new
                        //            {
                        //                Id = p.Id,
                        //                Alias = p.Alias,
                        //                DiscountPercent = p.DiscountPercent,
                        //                DiscountPrice = p.DiscountPrice,
                        //                Image = p.Image,
                        //                Price = p.Price,
                        //                Name = p.Name,
                        //                ParentName = m.Name
                        //            };
                        //query = query1;

                        query = query.Where(x => checkproduct.Contains(x.Id));
                    }
                    else
                        query = query.Where(x => x.Name.Contains(keySearch));
                }
                    
                                            
                if (!string.IsNullOrWhiteSpace(Typeid))
                {
                    var checkproduct = context.ProductAttributes.Where(x => x.ValueString.Contains(Typeid)).Select(x => x.ProductId).Distinct();
                        query = query.Where(x=>checkproduct.Contains(x.Id));
                }

                //query = query.OrderBy(x => x.Price);
                //query = query.Where(x => x.Price > 10000000);
                if (!string.IsNullOrWhiteSpace(orderBy))
                {
                    switch (orderBy)
                    {
                        case "name-asc":
                            query = query.OrderBy(x => x.Name);
                            break;
                        case "name-desc":
                            query = query.OrderByDescending(x => x.Name);
                            break;
                        case "price-asc":
                            query = query.OrderBy(x => x.Price);
                            break;
                        case "price-desc":
                            query = query.OrderByDescending(x => x.Price);
                            break;
                    }
                }
                if (!string.IsNullOrWhiteSpace(price))
                {
                    switch (price)
                    {
                        case "price-id1":
                            query = query.Where(x => x.Price <= 2000000);
                            break;
                        case "price-id2":
                            query = query.Where(x => x.Price > 2000000 & x.Price <= 10000000);
                            break;
                        case "price-id3":
                            query = query.Where(x => x.Price > 10000000 & x.Price <= 20000000);
                            break;
                        case "price-id4":
                            query = query.Where(x => x.Price > 20000000);
                            break;
                    }
                }
                totalRecord = query.Count();
                return query
                    .Select(x => new ProductDto()
                    {
                        Id = x.Id,
                        Alias = x.Alias,
                        DiscountPercent = x.DiscountPercent,
                        DiscountPrice = x.DiscountPrice,
                        Image = x.Image,
                        Price = x.Price,
                        Name = x.Name
                    }).Skip((page - 1) * pageSize).Take(pageSize).ToList();

            }
        }
        public virtual List<OrderByDto> GetStringOrderBy()
        {
            var list = new List<OrderByDto>();

            list.Add(new OrderByDto("Sắp xếp theo tên A-Z ", "name-asc"));
            list.Add(new OrderByDto("Sắp xếp theo tên Z-A", "name-desc"));
            list.Add(new OrderByDto("Sắp xếp theo giá tăng ", "price-asc"));
            list.Add(new OrderByDto("Sắp xếp theo giá giảm ", "price-desc"));
            return list;
        }

        public virtual List<string> GetBranch()
        {
            var list = new List<string>();
            using (var context = new MyContext())
            {
                var data = context.ProductAttributes.Where(x => x.AttributeId == 1).Select(x => x.ValueString).Distinct();
                list.AddRange(data);
            }
            return list;
        }

        public virtual List<OrderByDto> FilterPrice()
        {
            var list = new List<OrderByDto>();
            list.Add(new OrderByDto("0-2.000.000", "price-id1"));
            list.Add(new OrderByDto("2.000.000-10.000.000", "price-id2"));
            list.Add(new OrderByDto("10.000.000-20.000.000", "price-id3"));
            list.Add(new OrderByDto("trên 20.000.000", "price-id4"));
            return list;
        }
        public virtual ProductDto GetById(int key)
        {
            using (var context = new MyContext())
            {
                return context.Products
                    .Where(x => x.Id == key)
                    .Select(x => new ProductDto()
                    {
                        Id = x.Id,
                        Alias = x.Alias,
                        CategoryId = x.CategoryId,
                        Created = x.Created,
                        Description = x.Description,
                        DiscountPercent = x.DiscountPercent,
                        DiscountPrice = x.DiscountPrice,
                        Selling = x.Selling,
                        Image = x.Image,
                        Index = x.Index,
                        MenuId = x.MenuId,
                        Price = x.Price,
                        Name = x.Name,
                        ShortDescription = x.ShortDescription,
                        Status = x.Status,
                        MetaContentLanguage = x.MetaContentLanguage,
                        MetaContentType = x.MetaContentType,
                        MetaDescription = x.MetaDescription,
                        MetaRevisitAfter = x.MetaRevisitAfter,
                        MetaRobots = x.MetaRobots,
                        UserDef1 = x.UserDef1,
                        UserDef2 = x.UserDef2,
                        UserDef3 = x.UserDef3,
                        UserDef4 = x.UserDef4,
                        UserDef5 = x.UserDef5,
                        Tags = x.Tags,
                        ProductAttributes = x.ProductAttributes.Select(y => new ProductAttributeDto()
                        {
                            AttributeId = y.AttributeId,
                            UserDef1 = y.UserDef1,
                            UserDef2 = y.UserDef2,
                            UserDef3 = y.UserDef3,
                            UserDef4 = y.UserDef4,
                            UserDef5 = y.UserDef5,
                            ValueNumber = y.ValueNumber,
                            ValueString = y.ValueString,
                            Index = y.Index,
                        }).ToList(),
                        ProductRelateds = x.ProductRelateds.Select(y => new ProductRelatedDto()
                        {
                            ProductRelatedId = y.ProductRelatedId,
                            Index = y.Index,
                        }).ToList(),
                        ProductImages = x.ProductImages.Select(y => new ProductImageDto()
                        {
                            Image = y.Image,
                            Index = y.Index
                        }).ToList(),
                    })
                    .FirstOrDefault();
            }
        }

        public virtual ProductDto GetByAlias(string alias)
        {
            using (var context = new MyContext())
            {
                return context.Products
                    .Where(x => x.Alias == alias)
                    .Select(x => new ProductDto()
                    {
                        Id = x.Id,
                        Alias = x.Alias,
                        CategoryId = x.CategoryId,
                        Created = x.Created,
                        Description = x.Description,
                        DiscountPercent = x.DiscountPercent,
                        DiscountPrice = x.DiscountPrice,
                        Selling = x.Selling,
                        Image = x.Image,
                        Index = x.Index,
                        MenuId = x.MenuId,
                        Price = x.Price,
                        Name = x.Name,
                        ShortDescription = x.ShortDescription,
                        Status = x.Status,
                        MetaContentLanguage = x.MetaContentLanguage,
                        MetaContentType = x.MetaContentType,
                        MetaDescription = x.MetaDescription,
                        MetaRevisitAfter = x.MetaRevisitAfter,
                        MetaRobots = x.MetaRobots,
                        UserDef1 = x.UserDef1,
                        UserDef2 = x.UserDef2,
                        UserDef3 = x.UserDef3,
                        UserDef4 = x.UserDef4,
                        UserDef5 = x.UserDef5,
                        Tags = x.Tags,
                        ProductAttributes = x.ProductAttributes.Select(y => new ProductAttributeDto()
                        {
                            Attribute = new AttributeDto()
                            {
                                Name = y.Attribute.Name
                            },
                            AttributeId = y.AttributeId,
                            UserDef1 = y.UserDef1,
                            UserDef2 = y.UserDef2,
                            UserDef3 = y.UserDef3,
                            UserDef4 = y.UserDef4,
                            UserDef5 = y.UserDef5,
                            ValueNumber = y.ValueNumber,
                            ValueString = y.ValueString,
                            Index = y.Index,
                        }).ToList(),
                        ProductRelateds = x.ProductRelateds.Select(y => new ProductRelatedDto()
                        {
                            ProductRelatedId = y.ProductRelatedId,
                            Index = y.Index,
                        }).ToList(),
                        ProductImages = x.ProductImages.Select(y => new ProductImageDto()
                        {
                            Image = y.Image,
                            Index = y.Index
                        }).ToList(),
                    })
                    .FirstOrDefault();
            }
        }

        public virtual ProductDto Insert(ProductDto entity)
        {
            using (var context = new MyContext())
            {
                Product product = new Product()
                {
                    Price = entity.Price,
                    Name = entity.Name,
                    MenuId = entity.MenuId,
                    Index = entity.Index,
                    Image = entity.Image,
                    DiscountPrice = entity.DiscountPrice,
                    DiscountPercent = entity.DiscountPercent,
                    Selling = entity.Selling,
                    Description = entity.Description,
                    Created = DateTime.Now,
                    CategoryId = entity.CategoryId,
                    Alias = "",
                    ShortDescription = entity.ShortDescription,
                    Status = entity.Status,
                    MetaContentLanguage = entity.MetaContentLanguage,
                    MetaContentType = entity.MetaContentType,
                    MetaDescription = entity.MetaDescription,
                    MetaRevisitAfter = entity.MetaRevisitAfter,
                    MetaRobots = entity.MetaRobots,
                    UserDef1 = entity.UserDef1,
                    Tags = entity.Tags,
                    UserDef2 = entity.UserDef2,
                    UserDef3 = entity.UserDef3,
                    UserDef4 = entity.UserDef4,
                    UserDef5 = entity.UserDef5
                };

                if (entity.ProductImages != null && entity.ProductImages.Count > 0)
                {
                    product.ProductImages = entity.ProductImages.Select(x => new ProductImage()
                    {
                        Image = x.Image,
                    }).ToList();
                }
                if (entity.ProductAttributes != null && entity.ProductAttributes.Count > 0)
                {
                    product.ProductAttributes = entity.ProductAttributes.Select(x => new ProductAttribute()
                    {
                        AttributeId = x.AttributeId,
                        UserDef1 = x.UserDef1,
                        UserDef2 = x.UserDef2,
                        UserDef3 = x.UserDef3,
                        UserDef4 = x.UserDef4,
                        UserDef5 = x.UserDef5,
                        ValueNumber = x.ValueNumber,
                        ValueString = x.ValueString,
                    }).ToList();
                }
                if (entity.ProductRelateds != null && entity.ProductRelateds.Count > 0)
                {
                    product.ProductRelateds = entity.ProductRelateds.Select(x => new ProductRelated()
                    {
                        ProductRelatedId = x.ProductRelatedId,
                    }).ToList();
                }

                context.Products.Add(product);
                context.SaveChanges();
                product.Alias = DataHelper.Unsign(product.Name) + "-" + product.Id;
                context.SaveChanges();

                return entity;
            }
        }

        public virtual void Update(int key, ProductDto entity)
        {
            using (var context = new MyContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    Product product = context.Products.FirstOrDefault(x => x.Id == key);

                    product.MenuId = entity.MenuId;
                    product.CategoryId = entity.CategoryId;
                    product.Name = entity.Name;
                    product.Alias = DataHelper.Unsign(entity.Name) + "-" + key;
                    product.Image = entity.Image;
                    product.Index = entity.Index;
                    product.Status = entity.Status;
                    product.Price = entity.Price;
                    product.DiscountPrice = entity.DiscountPrice;
                    product.DiscountPercent = entity.DiscountPercent;
                    product.Selling = entity.Selling;
                    product.Tags = entity.Tags;
                    product.ShortDescription = entity.ShortDescription;
                    product.Description = entity.Description;
                    product.MetaContentLanguage = entity.MetaContentLanguage;
                    product.MetaContentType = entity.MetaContentType;
                    product.MetaDescription = entity.MetaDescription;
                    product.MetaRevisitAfter = entity.MetaRevisitAfter;
                    product.MetaRobots = entity.MetaRobots;
                    product.UserDef1 = entity.UserDef1;
                    product.UserDef2 = entity.UserDef2;
                    product.UserDef3 = entity.UserDef3;
                    product.UserDef4 = entity.UserDef4;
                    product.UserDef5 = entity.UserDef5;

                    context.ProductAttributes.RemoveRange(product.ProductAttributes);
                    context.ProductImages.RemoveRange(product.ProductImages);
                    context.ProductRelateds.RemoveRange(product.ProductRelateds);

                    if (entity.ProductAttributes != null && entity.ProductAttributes.Count > 0)
                    {
                        context.ProductAttributes.AddRange(entity.ProductAttributes.Select(x => new ProductAttribute()
                        {
                            AttributeId = x.AttributeId,
                            ProductId = key,
                            Index = x.Index,
                            UserDef1 = x.UserDef1,
                            UserDef2 = x.UserDef2,
                            UserDef3 = x.UserDef3,
                            UserDef4 = x.UserDef4,
                            UserDef5 = x.UserDef5,
                            ValueNumber = x.ValueNumber,
                            ValueString = x.ValueString,
                        }));
                    }

                    if (entity.ProductImages != null && entity.ProductImages.Count > 0)
                    {
                        context.ProductImages.AddRange(entity.ProductImages.Select(x => new ProductImage()
                        {
                            ProductId = key,
                            Image = x.Image,
                            Index = x.Index
                        }));
                    }

                    if (entity.ProductRelateds != null && entity.ProductRelateds.Count > 0)
                    {
                        context.ProductRelateds.AddRange(entity.ProductRelateds.Select(x => new ProductRelated()
                        {
                            ProductId = key,
                            ProductRelatedId = x.ProductRelatedId,
                            Index = x.Index
                        }));
                    }

                    context.SaveChanges();
                    transaction.Commit();
                }
            }
        }



    }
}
