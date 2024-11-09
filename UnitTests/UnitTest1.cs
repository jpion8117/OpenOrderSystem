using PizzaParty.Data.DataModels;

namespace UnitTests
{
    public class Tests
    {
        //sample test data.
        private List<Ingredient> _ingredients = new List<Ingredient>();
        private List<MenuItem> _menuItems = new List<MenuItem>();

        [SetUp]
        public void Setup()
        {
            //create some test ingredients.
            _ingredients.Add(new Ingredient()
            {
                Id = 1,
                Name = "Ingredient 1",
                Price = 1.5f
            });
            _ingredients.Add(new Ingredient()
            {
                Id = 2,
                Name = "Ingredient 2",
                Price = 1.5f
            });
            _ingredients.Add(new Ingredient()
            {
                Id = 3,
                Name = "Ingredient 3",
                Price = 1.5f
            });
            _ingredients.Add(new Ingredient()
            {
                Id = 4,
                Name = "Ingredient 4",
                Price = 1.5f
            });

            //create test menu items
            _menuItems.Add(new MenuItem()
            {
                Id = 1,
                Name = "All Ingredients",
                MenuItemVarients = new List<MenuItemVarient>
                {
                    new MenuItemVarient()
                    {
                        Price = 10.99f
                    }
                },
                Ingredients = new List<MenuItemIngredient>
                {
                    new MenuItemIngredient()
                    {

                    }
                }
            }); 
            _menuItems.Add(new MenuItem()
            {
                Id = 2,
                Name = "Ingredient 1 only",
                MenuItemVarients = new List<MenuItemVarient>
                {
                    new MenuItemVarient
                    {
                        Price = 9.99f
                    }
                },
                Ingredients = new List<Ingredient>
                {
                    _ingredients[0]
                }
            });
            _menuItems.Add(new MenuItem()
            {
                Id = 3,
                Name = "All Ingredients except #3",
                MenuItemVarients = new List<MenuItemVarient>
                {
                    new MenuItemVarient
                    {
                        Price = 14.99f
                    }
                },
                Ingredients = new List<Ingredient>
                {
                    _ingredients[0],
                    _ingredients[1],
                    _ingredients[3]
                }
            });
        }

        [Test]
        public void RemovedOneIngredient()
        {
            var expected = new
            {
                subtotal = 10.99f,
                tax = 10.99f * 0.06f,
                total = (10.99f * 0.06f) + 10.99f
            };

            var order = new Order
            {
                LineItems = new List<OrderLine>
                {
                    new OrderLine
                    {
                        MenuItem = _menuItems[0],
                        Ingredients = _menuItems[0].Ingredients.Take(3).ToList()
                    }
                }
            };

            PrintOrderTestState(order);

            Assert.That(order.Subtotal, Is.EqualTo(expected.subtotal),
                $"Subtotal expected: {expected.subtotal}, actual: {order.Subtotal}");
            
            Assert.That(order.Tax, Is.EqualTo(expected.tax),
                $"Tax expected: {expected.tax}, actual: {order.Tax}");

            Assert.That(order.Total, Is.EqualTo(expected.total),
                $"Total expected: {expected.total}, actual: {order.Total}");

            Assert.Pass();
        }

        [Test]
        public void AddOneIngredient()
        {
            var expected = new
            {
                subtotal = 14.99f + 1.5f,
                tax = (14.99f + 1.5f) * 0.06f,
                total = ((14.99f + 1.5f) * 0.06f) + (14.99f + 1.5f)
            };

            var order = new Order
            {
                LineItems = new List<OrderLine>
                {
                    new OrderLine
                    {
                        MenuItem = _menuItems[2],
                        Ingredients = _menuItems[0].Ingredients
                    }
                }
            };

            PrintOrderTestState(order);

            Assert.That(order.Subtotal, Is.EqualTo(expected.subtotal),
                $"Subtotal expected: {expected.subtotal}, actual: {order.Subtotal}");

            Assert.That(order.Tax, Is.EqualTo(expected.tax),
                $"Tax expected: {expected.tax}, actual: {order.Tax}");

            Assert.That(order.Total, Is.EqualTo(expected.total),
                $"Total expected: {expected.total}, actual: {order.Total}");

            Assert.Pass();
        }

        [Test]
        public void ThreeBasicItems()
        {
            var prices = 14.99f + 14.99f + 10.99f;
            var expected = new
            {
                subtotal = prices,
                tax = prices * 0.06f,
                total = (prices * 0.06f) + prices
            };

            var order = new Order
            {
                LineItems = new List<OrderLine>
                {
                    new OrderLine
                    {
                        MenuItem = _menuItems[2],
                        Ingredients = _menuItems[2].Ingredients
                    },
                    new OrderLine
                    {
                        MenuItem = _menuItems[2],
                        Ingredients = _menuItems[2].Ingredients
                    },
                    new OrderLine
                    {
                        MenuItem = _menuItems[0],
                        Ingredients = _menuItems[0].Ingredients
                    }
                }
            };

            PrintOrderTestState(order);

            Assert.That(order.Subtotal, Is.EqualTo(expected.subtotal),
                $"Subtotal expected: {expected.subtotal}, actual: {order.Subtotal}");

            Assert.That(order.Tax, Is.EqualTo(expected.tax),
                $"Tax expected: {expected.tax}, actual: {order.Tax}");

            Assert.That(order.Total, Is.EqualTo(expected.total),
                $"Total expected: {expected.total}, actual: {order.Total}");

            Assert.Pass();
        }

        [Test]
        public void TwoBasicItemsOneAdditionPlusTwo()
        {
            var prices = 14.99f + 9.99f + 10.99f + 1.5f + 1.5f;
            var expected = new
            {
                subtotal = prices,
                tax = prices * 0.06f,
                total = (prices * 0.06f) + prices
            };

            var order = new Order
            {
                LineItems = new List<OrderLine>
                {
                    new OrderLine
                    {
                        MenuItem = _menuItems[2],
                        Ingredients = _menuItems[2].Ingredients
                    },
                    new OrderLine
                    {
                        MenuItem = _menuItems[1],
                        Ingredients = new List<Ingredient>(_menuItems[1].Ingredients)
                    },
                    new OrderLine
                    {
                        MenuItem = _menuItems[0],
                        Ingredients = _menuItems[0].Ingredients
                    }
                }
            };

            //add two items
            order.LineItems[1].Ingredients.Add(_ingredients[1]);
            order.LineItems[1].Ingredients.Add(_ingredients[2]);

            PrintOrderTestState(order);

            Assert.That(order.LineItems[1].AddedIngredients.Count, Is.EqualTo(2),
                $"Modified Line Item: " +
                $"IngredientCount {order.LineItems[1].Ingredients.Count}," +
                $"AddedIngredientsCount {order.LineItems[1].AddedIngredients.Count()}" +
                $"RemovedIngredientsCount {order.LineItems[1].RemovedIngredients.Count()}");

            Assert.That(order.Subtotal, Is.EqualTo(expected.subtotal),
                $"Subtotal expected: {expected.subtotal}, actual: {order.Subtotal}");

            Assert.That(order.Tax, Is.EqualTo(expected.tax),
                $"Tax expected: {expected.tax}, actual: {order.Tax}");

            Assert.That(order.Total, Is.EqualTo(expected.total),
                $"Total expected: {expected.total}, actual: {order.Total}");

            Assert.Pass();
        }

        private void PrintOrderTestState(Order order)
        {
            //add state to log
            TestContext.WriteLine("*** Test State ***");
            for (int i = 0; i < order.LineItems.Count; i++)
            {
                TestContext.WriteLine($"Order line {i + 1}.");
                TestContext.WriteLine($"\tMenu Item:{order.LineItems[i].MenuItem.Name}");
                TestContext.WriteLine($"\tBase Price:{order.LineItems[i].MenuItem.Price}");

                TestContext.WriteLine($"\tBase Ingredients List: ({order.LineItems[i].MenuItem.Ingredients.Count} items)");
                for (int j = 0; j < order.LineItems[i].MenuItem.Ingredients.Count; j++)
                {
                    TestContext.WriteLine($"\t\t{order.LineItems[i].MenuItem.Ingredients[j].Name}");
                }

                TestContext.WriteLine($"\tIngredients List: ({order.LineItems[i].MenuItem.Ingredients.Count} items)");
                for (int j = 0; j < order.LineItems[i].Ingredients.Count; j++)
                {
                    TestContext.WriteLine($"\t\t{order.LineItems[i].Ingredients[j].Name}");
                }

                TestContext.WriteLine($"\tAdded Ingredients List: ({order.LineItems[i].AddedIngredients.Count} items)");
                for (int j = 0; j < order.LineItems[i].AddedIngredients.Count; j++)
                {
                    TestContext.WriteLine($"\t\t{order.LineItems[i].AddedIngredients[j].Name}");
                }
                TestContext.WriteLine($"\tRemoved Ingredients List: ({order.LineItems[i].RemovedIngredients.Count} items)");
                for (int j = 0; j < order.LineItems[i].RemovedIngredients.Count; j++)
                {
                    TestContext.WriteLine($"\t\t{order.LineItems[i].RemovedIngredients[j].Name}");
                }

                TestContext.WriteLine($"\tLine Price:{order.LineItems[i].LinePrice}");
            }

            TestContext.WriteLine($"\n\nOrder Totals");
            TestContext.WriteLine($"\tSubtotal: {order.Subtotal}");
            TestContext.WriteLine($"\tTax: {order.Tax}");
            TestContext.WriteLine($"\tTotal: {order.Total}");
        }
    }
}