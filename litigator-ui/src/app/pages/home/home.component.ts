import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, forkJoin } from 'rxjs';
import { takeUntil, catchError } from 'rxjs/operators';
import { of } from 'rxjs';

// DevExtreme imports
import { DxDataGridModule, DxButtonModule, DxLoadIndicatorModule, DxPieChartModule, DxChartModule } from 'devextreme-angular';

import { DashboardMetrics, AttorneyPerformanceDTO, CriticalCaseDTO, MonthlyTrendDTO } from '../../models/analytics.model';
import { Case, CaseStatus } from '../../models/case.model';
import { Person, PersonType, LegalSpecialization } from '../../models/person.model';
import { AttorneyDTO } from '../../models/attorney.model';
import { ClientDTO } from '../../models/client.model';
import { Deadline } from '../../models/deadline.model';

import { AttorneyService } from '../../services/attorney.service';
import { PersonService } from '../../services/person.service';
import { AnalyticsService } from '../../services/analytics.service';
import { DeadlineService } from '../../services/deadline.service';
import { CaseService } from '../../services/case.service';
import { ClientService } from '../../services/client.service';

@Component({
  selector: 'app-home',
  standalone: true, // Make it standalone
  imports: [
    CommonModule,
    DxDataGridModule,
    DxButtonModule,
    DxLoadIndicatorModule,
    DxPieChartModule,
    DxChartModule
  ],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  title = 'Litigator Dashboard';
  isLoading = false;
  loadingError = false;
  errorMessage = '';

  // Dashboard metrics
  metrics: DashboardMetrics = {
    totalActiveCases: 0,
    upcomingDeadlines: 0,
    criticalDeadlines: 0,
    totalClients: 0,
    monthlyRevenue: 0,
    averageCaseValue: 0
  };

  // Data for tables
  recentCases: Case[] = [];
  upcomingDeadlines: Deadline[] = [];
  criticalCases: CriticalCaseDTO[] = [];
  attorneyPerformance: AttorneyPerformanceDTO[] = [];
  monthlyTrends: MonthlyTrendDTO[] = [];
  attorneys: AttorneyDTO[] = [];
  attorneyWorkload: any[] = [];

  // Chart data
  casesByStatus: any[] = [];
  casesByType: any[] = [];

  // DevExtreme data grid columns
  recentCasesColumns = [
    { dataField: 'caseNumber', caption: 'Case #', width: 120 },
    { dataField: 'caseTitle', caption: 'Title' },
    { dataField: 'caseType', caption: 'Type', width: 120 },
    { dataField: 'status', caption: 'Status', width: 100, cellTemplate: 'statusTemplate' },
    { dataField: 'estimatedValue', caption: 'Value', width: 120, format: 'currency' }
  ];

  upcomingDeadlinesColumns = [
    { dataField: 'deadlineType', caption: 'Type', width: 150 },
    { dataField: 'description', caption: 'Description' },
    { dataField: 'deadlineDate', caption: 'Due Date', width: 120, dataType: 'date' },
    { dataField: 'daysUntil', caption: 'Days Left', width: 100 },
    { dataField: 'isCritical', caption: 'Priority', width: 100, cellTemplate: 'priorityTemplate' }
  ];

  clients: ClientDTO[] = [];

  constructor(
    private attorneyService: AttorneyService,
    private personService: PersonService,
    private analyticsService: AnalyticsService,
    private deadlineService: DeadlineService,
    private caseService: CaseService,
    private clientService: ClientService 
  ) { }

  ngOnInit(): void {
    this.loadDashboardData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadDashboardData(): void {
    this.isLoading = true;
    this.loadingError = false;
    this.errorMessage = '';

    // Use forkJoin to load all data simultaneously and handle errors properly
    forkJoin({
      attorneys: this.attorneyService.getActiveAttorneys().pipe(
        catchError(error => {
          console.error('Error loading attorneys:', error);
          return of([]);
        })
      ),
      clients: this.clientService.getActiveClients().pipe(  // Add this
        catchError(error => {
          console.error('Error loading clients:', error);
          return of([]);
        })
      ),
      attorneyPerformance: this.analyticsService.getAttorneyPerformance().pipe(
        catchError(error => {
          console.error('Error loading attorney performance:', error);
          return of([]);
        })
      ),
      criticalCases: this.analyticsService.getCriticalCases().pipe(
        catchError(error => {
          console.error('Error loading critical cases:', error);
          return of([]);
        })
      ),
      monthlyTrends: this.analyticsService.getMonthlyTrends().pipe(
        catchError(error => {
          console.error('Error loading monthly trends:', error);
          return of([]);
        })
      ),
      upcomingDeadlines: this.deadlineService.getUpcomingDeadlines(30).pipe(
        catchError(error => {
          console.error('Error loading upcoming deadlines:', error);
          return of([]);
        })
      ),
      recentCases: this.caseService.getCases(1, 10).pipe(
        catchError(error => {
          console.error('Error loading recent cases:', error);
          return of({ data: [], totalCount: 0, pageNumber: 1, pageSize: 10, totalPages: 0 });
        })
      )
    }).pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (results) => {
        console.log('Raw results:', results)

        this.attorneys = results.attorneys;
        this.clients = results.clients;
        this.attorneyPerformance = results.attorneyPerformance;
        this.criticalCases = results.criticalCases;
        this.monthlyTrends = results.monthlyTrends;
        this.upcomingDeadlines = results.upcomingDeadlines;
        this.recentCases = results.recentCases.data || results.recentCases;

        // Generate derived data
        this.generateAttorneyWorkload();
        this.generateChartData();
        this.calculateMetrics();

        this.isLoading = false;
        console.log('Dashboard data loaded successfully');
      },
      error: (error) => {
        console.error('Error loading dashboard data:', error);
        this.loadingError = true;
        this.errorMessage = 'Failed to load dashboard data. Please try again.';
        this.isLoading = false;

        // Load fallback mock data
        this.loadMockData();
      }
    });
  }

  private generateAttorneyWorkload(): void {
    this.attorneyWorkload = this.attorneys.map(attorney => ({
      attorney: attorney.name?.display || `${attorney.name?.first} ${attorney.name?.last}`,
      activeCases: this.attorneyPerformance.find(perf => perf.attorneyName === attorney.name?.display)?.activeCases || 0
    }));
  }

  private generateChartData(): void {
    // Generate cases by status from recent cases
    const statusCounts = this.recentCases.reduce((acc, case_) => {
      acc[case_.status] = (acc[case_.status] || 0) + 1;
      return acc;
    }, {} as Record<string, number>);

    this.casesByStatus = Object.entries(statusCounts).map(([status, count]) => ({
      status,
      count,
      color: this.getStatusColor(status)
    }));

    // Generate cases by type from recent cases
    const typeCounts = this.recentCases.reduce((acc, case_) => {
      acc[case_.caseType] = (acc[case_.caseType] || 0) + 1;
      return acc;
    }, {} as Record<string, number>);

    this.casesByType = Object.entries(typeCounts).map(([type, count]) => ({
      type,
      count,
      color: this.getTypeColor(type)
    }));
  }

  private getStatusColor(status: string): string {
    switch (status) {
      case 'Active': return '#28a745';
      case 'Pending': return '#ffc107';
      case 'Open': return '#17a2b8';
      case 'Closed': return '#6c757d';
      default: return '#6c757d';
    }
  }

  private getTypeColor(type: string): string {
    const colors = ['#007bff', '#28a745', '#ffc107', '#dc3545', '#6f42c1', '#fd7e14'];
    const index = type.length % colors.length;
    return colors[index];
  }

  private calculateMetrics(): void {
    this.metrics = {
      totalActiveCases: this.recentCases.filter(c => c.status === CaseStatus.Active).length,
      upcomingDeadlines: this.upcomingDeadlines.length,
      criticalDeadlines: this.upcomingDeadlines.filter(d => d.isCritical).length,
      totalClients: this.clients.length,
      monthlyRevenue: this.monthlyTrends.length > 0 ? this.monthlyTrends[this.monthlyTrends.length - 1].totalRevenue : 0,
      averageCaseValue: this.monthlyTrends.length > 0 ? this.monthlyTrends[this.monthlyTrends.length - 1].avgCaseValue : 0
    };
  }


  private loadMockData(): void {
    // Mock metrics
    this.metrics = {
      totalActiveCases: 42,
      upcomingDeadlines: 15,
      criticalDeadlines: 3,
      totalClients: 96,
      monthlyRevenue: 125000,
      averageCaseValue: 45000
    };

    // Mock clients
    this.clients = Array.from({ length: 96 }, (_, i) => ({
      clientId: i + 1,
      name: `Client ${i + 1}`,
      email: `client${i + 1}@example.com`,
      primaryPhone: `(555) ${String(i + 1).padStart(3, '0')}-${String(i + 1).padStart(4, '0')}`,
      primaryAddress: `${i + 1} Main St, City, State`,
      isActive: Math.random() > 0.1, // 90% active
      createdDate: new Date(2023, Math.floor(Math.random() * 12), Math.floor(Math.random() * 28) + 1),
      totalCases: Math.floor(Math.random() * 5) + 1,
      activeCases: Math.floor(Math.random() * 3)
    }));

    // Mock recent cases
    this.recentCases = [
      {
        caseId: 1,
        caseNumber: 'LIT-2024-001',
        caseTitle: 'Smith vs. Johnson Construction',
        caseType: 'Personal Injury',
        filingDate: new Date('2024-01-15'),
        status: CaseStatus.Active,
        estimatedValue: 75000,
        currentCost: 12000,
        clientId: 1,
        assignedAttorneyId: 1,
        assignedJudgeId: 1,
        courtId: 1,
        deadlines: [],
        daysActive: 145
      },
      {
        caseId: 2,
        caseNumber: 'LIT-2024-002',
        caseTitle: 'ABC Corp Contract Dispute',
        caseType: 'Corporate Law',
        filingDate: new Date('2024-02-01'),
        status: CaseStatus.Active,
        estimatedValue: 150000,
        currentCost: 8500,
        clientId: 2,
        assignedAttorneyId: 2,
        assignedJudgeId: 2,
        courtId: 1,
        deadlines: [],
        daysActive: 128
      },
      {
        caseId: 3,
        caseNumber: 'LIT-2024-003',
        caseTitle: 'Miller Family Custody',
        caseType: 'Family Law',
        filingDate: new Date('2024-03-10'),
        status: CaseStatus.Pending,
        estimatedValue: 25000,
        currentCost: 5200,
        clientId: 3,
        assignedAttorneyId: 3,
        assignedJudgeId: 3,
        courtId: 2,
        deadlines: [],
        daysActive: 90
      }
    ];

    // Mock upcoming deadlines
    this.upcomingDeadlines = [
      {
        deadlineId: 1,
        deadlineType: 'Motion Filing',
        description: 'File motion to dismiss',
        deadlineDate: new Date('2024-07-15'),
        isCompleted: false,
        isCritical: true,
        caseId: 1,
        daysUntil: 8
      },
      {
        deadlineId: 2,
        deadlineType: 'Discovery',
        description: 'Complete document discovery',
        deadlineDate: new Date('2024-07-20'),
        isCompleted: false,
        isCritical: false,
        caseId: 2,
        daysUntil: 13
      },
      {
        deadlineId: 3,
        deadlineType: 'Court Appearance',
        description: 'Custody hearing',
        deadlineDate: new Date('2024-07-12'),
        isCompleted: false,
        isCritical: true,
        caseId: 3,
        daysUntil: 5
      }
    ];

    // Mock monthly trends
    this.monthlyTrends = [
      { period: '2024-01', newCases: 5, totalRevenue: 95000, avgCaseValue: 19000 },
      { period: '2024-02', newCases: 7, totalRevenue: 110000, avgCaseValue: 15714 },
      { period: '2024-03', newCases: 6, totalRevenue: 125000, avgCaseValue: 20833 }
    ];

    // Generate chart data from mock data
    this.generateChartData();
    this.generateAttorneyWorkload();
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 0
    }).format(value);
  }

  getSpecializationText(specialization: LegalSpecialization): string {
    switch (specialization) {
      case LegalSpecialization.GeneralPractice: return 'General Practice';
      case LegalSpecialization.CriminalLaw: return 'Criminal Law';
      case LegalSpecialization.CivilLitigation: return 'Civil Litigation';
      case LegalSpecialization.FamilyLaw: return 'Family Law';
      case LegalSpecialization.CorporateLaw: return 'Corporate Law';
      case LegalSpecialization.RealEstateLaw: return 'Real Estate Law';
      case LegalSpecialization.PersonalInjury: return 'Personal Injury';
      case LegalSpecialization.EmploymentLaw: return 'Employment Law';
      case LegalSpecialization.ImmigrationLaw: return 'Immigration Law';
      case LegalSpecialization.BankruptcyLaw: return 'Bankruptcy Law';
      case LegalSpecialization.TaxLaw: return 'Tax Law';
      case LegalSpecialization.IntellectualProperty: return 'Intellectual Property';
      default: return 'Unknown';
    }
  }

  onCaseClick(caseData: Case): void {
    console.log('Case clicked:', caseData);
    // TODO: Navigate to case detail
  }

  onDeadlineClick(deadline: Deadline): void {
    console.log('Deadline clicked:', deadline);
    // TODO: Navigate to deadline detail
  }

  onAttorneyClick(attorney: AttorneyDTO): void {
    console.log('Attorney clicked:', attorney);
    // TODO: Navigate to attorney detail
  }

  createNewCase(): void {
    console.log('Create new case clicked');
    // TODO: Navigate to new case form
  }

  viewAllCases(): void {
    console.log('View all cases clicked');
    // TODO: Navigate to cases list
  }

  viewAllDeadlines(): void {
    console.log('View all deadlines clicked');
    // TODO: Navigate to deadlines list
  }

  refresh(): void {
    console.log('Refresh clicked');
    this.loadDashboardData();
  }

  getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'Active': return 'badge-success';
      case 'Pending': return 'badge-warning';
      case 'Open': return 'badge-info';
      case 'Closed': return 'badge-secondary';
      default: return 'badge-secondary';
    }
  }

  getPriorityBadgeClass(deadline: Deadline): string {
    if (deadline.isCritical) {
      return 'badge-danger';
    }
    if (deadline.daysUntil && deadline.daysUntil <= 7) {
      return 'badge-warning';
    }
    return 'badge-info';
  }

  getPriorityText(deadline: Deadline): string {
    if (deadline.isCritical) {
      return 'Critical';
    }
    if (deadline.daysUntil && deadline.daysUntil <= 7) {
      return 'Urgent';
    }
    return 'Normal';
  }

  getScoreBadgeClass(score: number): string {
    if (score >= 8) return 'badge-success';
    if (score >= 6) return 'badge-warning';
    return 'badge-danger';
  }

  getPriorityScoreBadgeClass(score: number): string {
    if (score >= 8) return 'badge-danger';
    if (score >= 6) return 'badge-warning';
    return 'badge-info';
  }

  // DevExtreme event handlers
  onCaseRowClick(e: any): void {
    this.onCaseClick(e.data);
  }

  onDeadlineRowClick(e: any): void {
    this.onDeadlineClick(e.data);
  }

  // Chart customization
  customizeLabel = (arg: any): string => {
    return `${arg.argumentText}: ${arg.valueText}`;
  }

  customizeTooltip = (arg: any): any => {
    return {
      text: `${arg.argumentText}: ${arg.valueText}`
    };
  }
}
