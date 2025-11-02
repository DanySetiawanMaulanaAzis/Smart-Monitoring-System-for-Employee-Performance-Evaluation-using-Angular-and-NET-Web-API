import { Component, OnInit, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { SelectModule } from 'primeng/select';
import { Employee } from '../../services/employee';
import { ChangeDetectorRef } from '@angular/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { provideMomentDateAdapter } from '@angular/material-moment-adapter';
import { MY_DATE_FORMATS } from '../../core/utils/date-format';
import { DateAdapter, MAT_DATE_FORMATS } from '@angular/material/core';
import { MAT_DATE_LOCALE } from '@angular/material/core';
import {
  MomentDateAdapter,
  MAT_MOMENT_DATE_ADAPTER_OPTIONS,
} from '@angular/material-moment-adapter';
import * as echarts from 'echarts';
import moment from 'moment';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatBadgeModule } from '@angular/material/badge';

import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { ViewChild } from '@angular/core';
declare var $: any;

@Component({
  selector: 'app-employee.component',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    SelectModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatInputModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatButtonModule,
    MatSelectModule,
    MatBadgeModule,
  ],
  providers: [
    {
      provide: DateAdapter,
      useClass: MomentDateAdapter,
      deps: [MAT_DATE_LOCALE, MAT_MOMENT_DATE_ADAPTER_OPTIONS],
    },
    { provide: MAT_DATE_FORMATS, useValue: MY_DATE_FORMATS },
  ],
  templateUrl: './employee.component.html',
  styleUrl: './employee.component.css',
})
export class EmployeeComponent implements OnInit, AfterViewInit {
  dashboard: any = {};
  workLogs: any[] = [];
  products: any[] = [];
  selectedProduct: string = '';
  startDate: Date = new Date();
  endDate: Date = new Date();
  activeTimers: { [workLogId: number]: number } = {}; // simpan detik berjalan
  loading = false;
  elapsedSeconds: number = 0;
  workLogId: number = 0; // ambil dari backend saat start log


  private chartInstance: echarts.ECharts | null = null;
  private timerInterval: any;
  private backendUpdateInterval: any;

  displayedColumns: string[] = ['no', 'productName', 'startTime', 'totalTime', 'statusName', 'actions'];
  dataSource = new MatTableDataSource<any>([]);

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;


  constructor(private Employee: Employee, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadDashboard();
    this.loadProducts();
    this.loadWorkLogs();

    // Default range: 7 hari terakhir
    const today = new Date();
    const sevenDaysAgo = new Date(today.getTime() - 8 * 24 * 60 * 60 * 1000);
    this.startDate = sevenDaysAgo;
    this.endDate = today;
  }

  ngAfterViewInit(): void {
    this.initChart();
    this.loadChart();
  }

  /** Dashboard Data */
  loadDashboard(): void {
    this.Employee.getDashboard().subscribe({
      next: (res) => {
        console.log('Dashboard response:', res); // ✅ cek di console
        this.dashboard = res;
        this.cdr.detectChanges(); // 👈 kasih tau Angular ada perubahan
      },
      error: (err) => console.error('Error loading dashboard:', err),
    });
  }

  /** WorkLogs */
  loadWorkLogs(): void {
  this.loading = true;
  this.Employee.getWorkLogs().subscribe({
    next: (res) => {
      this.workLogs = res;
      this.dataSource = new MatTableDataSource(res);
      this.dataSource.paginator = this.paginator;
      this.dataSource.sort = this.sort;
      this.loading = false;

      // ✅ Otomatis mulai timer untuk task yang belum selesai
      res.forEach(log => {
        if (log.statusName !== 'Completed') {
          this.startWorkLog(log.workLogId, log.totalTime || 0);
        }
      });
    },
    error: (err) => {
      console.error('Error loading worklogs:', err);
      this.loading = false;
    },
  });
}

  /** Products */
  loadProducts(): void {
    this.Employee.getProducts().subscribe({
      next: (res) => (this.products = res),
      error: (err) => console.error('Error loading products:', err),
    });
  }

  onProductChange(): void {
    if (this.selectedProduct) {
      const newWorkLog = { productId: +this.selectedProduct };

      this.Employee.createWorkLog(newWorkLog).subscribe({
        next: (newId) => {
          console.log('WorkLog created with ID:', newId);

          this.loadWorkLogs();
        },
        error: (err) => console.error('Error creating worklog:', err),
      });
    }
  }

  convertSeconds(seconds: number): string {
    if (!seconds) return '00:00:00';
    const h = Math.floor(seconds / 3600);
    const m = Math.floor((seconds % 3600) / 60);
    const s = seconds % 60;
    return `${h.toString().padStart(2, '0')}:${m.toString().padStart(2, '0')}:${s
      .toString()
      .padStart(2, '0')}`;
  }

  startWorkLog(workLogId: number, initialSeconds: number = 0) {

    if (this.timerInterval) clearInterval(this.timerInterval);
    if (this.backendUpdateInterval) clearInterval(this.backendUpdateInterval);

    this.workLogId = workLogId;
    this.elapsedSeconds = initialSeconds;
    this.activeTimers[workLogId] = initialSeconds; // mulai dari DB

    // UI update tiap detik
    this.timerInterval = setInterval(() => {
      this.elapsedSeconds++;
      this.activeTimers[workLogId]++;

      const log = this.workLogs.find((l) => l.workLogId === workLogId);
      if (log) {
        log.totalTime = this.activeTimers[workLogId];
      }

      this.cdr.detectChanges();
    }, 1000);

    // Push ke backend tiap 5 detik
    this.backendUpdateInterval = setInterval(() => {
      this.Employee.updateTotalTime(this.workLogId, 5).subscribe({
        error: (err) => console.error('Error updating total time:', err),
      });
    }, 5000);
  }

  stopWorkLog(workLogId: number) {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
      this.timerInterval = null;
    }
    if (this.backendUpdateInterval) {
      clearInterval(this.backendUpdateInterval);
      this.backendUpdateInterval = null;
    }

    // pastikan sync terakhir
    const finalSeconds = this.activeTimers[workLogId] || 0;
    if (finalSeconds > 0) {
      this.Employee.updateTotalTime(workLogId, finalSeconds % 5 || 5).subscribe({
        error: (err) => console.error('Final update error:', err),
      });
    }
  }

  // ini yang dipakai dan berhasil jadi jangan dihapus
  markAsCompleted(workLogId: number): void {
    const totalTime = this.activeTimers[workLogId] || 0;
    const payload = {
      workLogId: workLogId,
      totalTime: totalTime,
      markCompleted: true,
    };
    console.log('Update payload:', payload);

    // 🔹 Stop interval untuk workLog ini
    this.stopWorkLog(workLogId);

    this.Employee.updateWorkLog(payload).subscribe({
      next: () => {
        // ✅ update UI tanpa reload API
        const log = this.workLogs.find((l) => l.workLogId === workLogId);
        if (log) {
          log.statusName = 'Completed';
          log.endTime = new Date();
          log.totalTime = totalTime; // pakai waktu terakhir
        }

        // hapus dari activeTimers setelah stop
        delete this.activeTimers[workLogId];

        // 🔹 Refresh dashboard agar angka di kotak update
        this.loadDashboard();

        this.cdr.detectChanges();  
      },
      error: (err) => console.error('Error marking worklog as completed:', err),
    });
  }

  deleteWorkLog(workLogId: number): void {
    if (confirm('Are you sure you want to delete this work log?')) {
      this.Employee.deleteWorkLog(workLogId).subscribe({
        next: () => {
          this.loadWorkLogs();
        },
        error: (err) => console.error('Error deleting worklog:', err),
      });
    }
  }

  private formatDateForApi(date: Date): string {
    return moment(date).format('DD-MM-YYYY');
  }

  /** Chart */
  initChart(): void {
    const chartDom = document.getElementById('developmentChart')!;
    this.chartInstance = echarts.init(chartDom);
  }

  loadChart(): void {
    if (!this.startDate || !this.endDate) return;

    const formattedStart = this.formatDateForApi(this.startDate);
    const formattedEnd = this.formatDateForApi(this.endDate);

    this.Employee.getWorkLogChart(formattedStart, formattedEnd).subscribe({
      next: (res) => {
        if (this.chartInstance) {
          const option: echarts.EChartsOption = {
            title: { text: 'Total Work Per Day', left: 'center' },
            tooltip: {
              trigger: 'axis',
              axisPointer: { type: 'shadow' },
            },
            xAxis: { type: 'category', data: res.map((x: any) => x.label) },
            yAxis: { type: 'value' },
            series: [
              {
                name: 'Total Workmanship',
                type: 'bar',
                data: res.map((x: any) => x.data),
                label: { show: true, position: 'inside', color: '#fff' },
              },
              {
                name: 'Trend Garis',
                type: 'line',
                data: res.map((x: any) => x.data),
                smooth: true,
                lineStyle: { color: '#FF5733', width: 2 },
                symbol: 'circle',
                symbolSize: 8,
                tooltip: { show: false },
              },
            ],
          };

          this.chartInstance.setOption(option);
        }
      },
      error: (err) => console.error('Error loading chart:', err),
    });
  }
}
