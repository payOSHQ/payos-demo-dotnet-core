
from django.urls import path, include
from checkout.views import index, Checkout, success, cancel
urlpatterns = [
    path('', index),
    path('create-payment-link', Checkout.as_view()),
    path('success', success),
    path('cancel', cancel)
]
