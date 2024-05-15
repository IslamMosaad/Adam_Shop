﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutfitO.Models;
using OutfitO.Repository;
using System.Security.Claims;


namespace OutfitO.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        IOrderRepository orderRepository;
        IUserRepository userRepository;
        IOrderItemsRepository orderItemsRepository;

        public OrderController(IOrderRepository orderRepo, IUserRepository userRepository, IOrderItemsRepository orderItemsRepository)
        {
            this.orderRepository = orderRepo;
            this.userRepository = userRepository;
            this.orderItemsRepository = orderItemsRepository;
        }
        
        public IActionResult Add()
        {
            var Userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (decimal.TryParse(TempData["TPrice"] as string, out decimal price))
            {
                if (int.TryParse(TempData["Payment"] as string, out int paymentId))
                {
                    Order NewOrder = new Order()
                    {
                        Date = DateTime.Now,
                        UserId = Userid,
                        Price = price,
                        PaymentId = paymentId
                    };
                    orderRepository.Insert(NewOrder);
                    orderRepository.Save();
                    TempData["Order"] = NewOrder.Id.ToString();
                    return RedirectToAction("AddItems", "OrderItem");
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return NotFound();
            }
        }
        [Authorize(Roles ="Admin")]
        public IActionResult Index(int page = 1)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            User user = userRepository.GetUser(userId);
            int content = 8;
            int skip = (page - 1) * content;
            List<Order> orders = orderRepository.GetSomeOrders(skip, content);
            int total = orderRepository.Count();
            ViewData["Page"] = page;
            ViewData["content"] = content;
            ViewData["TotalItems"] = total;
            ViewData["User"] = user;
            return View("Index", orders);
        }

        public IActionResult OrderPagination(int page = 1)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            User user = userRepository.GetUser(userId);
            int content = 8;
            int skip = (page - 1) * content;
            List<Order> orders = orderRepository.GetSomeOrders(skip, content);
            int total = orderRepository.Count();
            ViewData["Page"] = page;
            ViewData["content"] = content;
            ViewData["TotalItems"] = total;
            ViewData["User"] = user;
            return PartialView("_OrderPaginationPartial", orders);
        }


        public IActionResult History(int page = 1)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            User user = userRepository.GetUser(userId);
            int content = 8;
            int skip = (page - 1) * content;
            List<Order> orders = orderRepository.GetSomeOrdersForUser(userId, skip, content);
            int total = orderRepository.CountOrdersForUser(userId);
            ViewData["Page"] = page;
            ViewData["content"] = content;
            ViewData["TotalItems"] = total;
            ViewData["User"] = user;
            return View("History", orders);
        }
        public IActionResult OrdersPaginationHistory(int page = 1)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            User user = userRepository.GetUser(userId);
            int content = 8;
            int skip = (page - 1) * content;
            List<Order> orders = orderRepository.GetSomeOrdersForUser(userId, skip, content);
            int total = orderRepository.CountOrdersForUser(userId);
            ViewData["Page"] = page;
            ViewData["content"] = content;
            ViewData["TotalItems"] = total;
            ViewData["User"] = user;
            return PartialView("_OrderPaginationHistoryPartial", orders);
        }

        public IActionResult Details(int id)
        {
            List<OrderItem> orderItems = orderRepository.GetOrderItem(id);
            return PartialView("_DetailsPartial", orderItems);
        }

    }
}
