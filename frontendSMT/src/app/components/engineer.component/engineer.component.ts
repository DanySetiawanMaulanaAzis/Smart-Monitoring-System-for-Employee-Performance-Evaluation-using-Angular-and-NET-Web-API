import { Component, OnInit, ViewChild } from '@angular/core';
import { Engineer } from '../../services/engineer';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DateFormatterService } from '../../core/utils/date-formatter';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatCardModule } from '@angular/material/card'; 
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import * as echarts from 'echarts';

@Component({
  selector: 'app-engineer.component',
  standalone: true,
  imports: [CommonModule, FormsModule, TableModule, ButtonModule, CardModule, MatTableModule, MatPaginatorModule, MatSortModule, MatCardModule, MatFormFieldModule, MatInputModule],
  templateUrl: './engineer.component.html',
  styleUrl: './engineer.component.css',
})
export class EngineerComponent implements OnInit {
  totalChart: any;
  userPerformanceChart: any;
  tableData: any[] = [];

  startDate1: string = '';
  endDate1: string = '';
  startDate2: string = '';
  endDate2: string = '';

  displayedColumns: string[] = [
    'username',
    'finishedToday',
    'averageWorkmanship',
    'totalWorkmanship',
    'predictedTomorrow',
    'performanceResult'
  ];
  dataSource = new MatTableDataSource<any>([]);

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(private engineerService: Engineer, private dateFormatter: DateFormatterService) {}

  ngOnInit(): void {
    this.initCharts();
    this.loadTotalChart();
    this.loadUserPerformanceChart();
    this.loadTable();
  }

  initCharts() {
    this.totalChart = echarts.init(document.getElementById('totalChart')!);
    this.userPerformanceChart = echarts.init(document.getElementById('userPerformanceChart')!);
  }

  loadTotalChart() {
    const start = this.dateFormatter.formatDateForApi(this.startDate1);
    const end = this.dateFormatter.formatDateForApi(this.endDate1);

    this.engineerService.getCompletedTasks(start, end).subscribe((data) => {
      const dates = data.map((x: any) =>
        this.dateFormatter.formatDateForUi(x.workDate.split('T')[0])
      );
      const completed = data.map((x: any) => x.completedTasks);

      this.totalChart.setOption({
        tooltip: { trigger: 'axis', axisPointer: { type: 'shadow' } },
        xAxis: { type: 'category', data: dates, axisLabel: { rotate: 45 } },
        yAxis: { type: 'value' },
        series: [
          {
            name: 'Total',
            type: 'bar',
            data: completed,
            itemStyle: { color: '#5470C6' },
            label: { show: true, position: 'inside', color: '#fff' },
          },
          {
            name: 'Tasks (Line)',
            type: 'line',
            data: completed,
            smooth: true,
            itemStyle: { color: '#FF5733', width: 2 },
            symbol: 'circle',
            symbolSize: 8,
            tooltip: { show: false },
          },
        ],
      });
    });
  }

  loadUserPerformanceChart() {
    this.engineerService.getUserPerformance(this.startDate2, this.endDate2).subscribe((data) => {
      const usernames = data.map((x: any) => x.username);
      const totals = data.map((x: any) => x.completedTasks);

      this.userPerformanceChart.setOption({
        tooltip: { trigger: 'axis', axisPointer: { type: 'shadow' } },
        xAxis: { type: 'category', data: usernames, axisLabel: { rotate: 45 } },
        yAxis: { type: 'value' },
        series: [
          {
            name: 'Total',
            type: 'bar',
            data: totals,
            itemStyle: { color: '#91CC75' },
            label: { show: true, position: 'inside', color: '#fff' },
          },
        ],
      });
    });
  }

  loadTable() {
    this.engineerService.getUserPerformanceTable().subscribe({
      next: (data) => {
        console.log('Data dari API:', data);
        this.dataSource = new MatTableDataSource(data);
        if (this.paginator) this.dataSource.paginator = this.paginator;
        if (this.sort) this.dataSource.sort = this.sort;
      },
      error: (err) => console.error('Error loadTable:', err),
    });
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();
  }
}
