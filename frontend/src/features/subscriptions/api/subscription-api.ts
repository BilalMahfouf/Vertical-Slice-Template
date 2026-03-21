import api from '@/lib/api/api';

export interface CheckoutResponse {
  checkoutUrl: string | null;
  subscriptionStatus: string | null;
  subscriptionId: string;
}

export interface SubscriptionPlan {
  id: string;
  name: string;
  slug: string;
  amount: number;
  currency: string;
  billingInterval: string;
  intervalCount: number;
  trialDays: number;
  isActive: boolean;
  createdOnUtc: string;
}

export interface MySubscriptionResponse {
  id: string;
  doctorId: string;
  planId: string;
  planName: string;
  planDisplayName: string;
  planPrice: number;
  planCurrency: string;
  subscriptionStatus: string;
  currentPeriodStart: string;
  currentPeriodEnd: string;
  trialEndsAt: string | null;
  cancelledAt: string | null;
  updatedAt: string | null;
  previousSubscriptionId: string | null;
}

interface CreateCheckoutRequest {
  planId: string;
}

const generateIdempotencyKey = (): string => {
  return `${Date.now()}-${Math.random().toString(36).substring(2, 11)}`;
};

const subscriptionApi = {
  getSubscriptionPlans: async (): Promise<SubscriptionPlan[]> => {
    const response = await api.get<SubscriptionPlan[]>('/subscription-plans');
    if (response.status !== 200) {
      throw new Error('Failed to fetch subscription plans');
    }
    return response.data;
  },

  getMySubscription: async (): Promise<MySubscriptionResponse> => {
    const response = await api.get<MySubscriptionResponse>('/subscriptions/me');
    if (response.status !== 200) {
      throw new Error('Failed to fetch subscription details');
    }
    return response.data;
  },

  createCheckout: async (planId: string): Promise<CheckoutResponse> => {
    const payload: CreateCheckoutRequest = { planId };

    const response = await api.post<CheckoutResponse>('/subscriptions', payload, {
      headers: {
        'Idempotency-Key': generateIdempotencyKey(),
      },
    });
    
    if (response.status !== 200 && response.status !== 201) {
      throw new Error('Failed to create checkout');
    }
    console.log('Checkout response:', response.data);
    return response.data;
  },
};

export default subscriptionApi;
