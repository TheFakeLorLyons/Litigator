export interface DashboardMetrics {
  totalActiveCases: number;
  upcomingDeadlines: number;
  criticalDeadlines: number;
  totalClients: number;
  monthlyRevenue: number;
  averageCaseValue: number;
}

export interface AttorneyPerformanceDTO {
  rank: number;
  attorneyName: string;
  barNumber: string;
  email: string;
  totalRevenue: number;
  avgCaseValue: number;
  completedCases: number;
  activeCases: number;
  totalCases: number;
  overdueDeadlines: number;
  completionRate: number;
  performanceScore: number;
}

export interface CaseOutcomePredictionDTO {
  caseNumber: string;
  caseTitle: string;
  clientName: string;
  attorneyName: string;
  daysOpen: number;
  completionRate: number;
  riskScore: number;
  predictedOutcome: string;
  estimatedValue: number;
}

export interface CriticalCaseDTO {
  caseNumber: string;
  caseTitle: string;
  clientName: string;
  attorneyName: string;
  caseValue: number;
  caseAge: number;
  nextDeadline: string;
  nextDeadlineDate?: Date;
  daysUntilDeadline?: number;
  overdueCount: number;
  attorneyWorkload: number;
  priorityScore: number;
}

export interface MonthlyTrendDTO {
  period: string;
  newCases: number;
  totalRevenue: number;
  avgCaseValue: number;
}

export interface DeadlinePerformanceDTO {
  caseNumber: string;
  attorneyName: string;
  totalDeadlines: number;
  completedOnTime: number;
  completedLate: number;
  stillPending: number;
  overdue: number;
  onTimePercentage: number;
}
