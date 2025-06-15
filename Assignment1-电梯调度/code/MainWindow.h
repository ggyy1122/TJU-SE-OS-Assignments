#ifndef MAINWINDOW_H
#define MAINWINDOW_H

#include <QMainWindow>
#include "Elevator.h"
#include "ElevatorWidget.h"
#include "FloorButtonsWidget.h"
#include "LogPanel.h"  
#include "Dispatcher.h" // 添加调度器头文件
class MainWindow : public QMainWindow
{
    Q_OBJECT

public:
    explicit MainWindow(QWidget* parent = nullptr);
    ~MainWindow();

private:
    Elevator* elevator1;                // 电梯对象
    ElevatorWidget* elevatorWidget1;    // 电梯显示控件
    Elevator* elevator2;                // 电梯对象
    ElevatorWidget* elevatorWidget2;    // 电梯显示控件
    Elevator* elevator3;                // 电梯对象
    ElevatorWidget* elevatorWidget3;    // 电梯显示控件
    Elevator* elevator4;                // 电梯对象
    ElevatorWidget* elevatorWidget4;    // 电梯显示控件
    Elevator* elevator5;                // 电梯对象
    ElevatorWidget* elevatorWidget5;    // 电梯显示控件
    FloorButtonsWidget* buttonsWidget; // 楼层按钮控件
    LogPanel* logPanel;                //日志控件
    Dispatcher* dispatcher;  // 调度器
};

#endif // MAINWINDOW_H
