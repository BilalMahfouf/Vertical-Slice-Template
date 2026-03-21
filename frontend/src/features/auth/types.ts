export type SubscriptionStatus = "Active" | "Inactive" | "Expired" | "Trialing";

export type CurrentUser = {
  id: string;
  userName: string;
  fullName: string;
  email: string;
  role: string;
  isActive: boolean;
  clinicInfromationCompleted: boolean;
  subscriptionStatus: SubscriptionStatus | null;
  isSubscriptionExist: boolean ;
  createdOnUtc: string;
};

