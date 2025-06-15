#ifndef MAINWINDOW_H
#define MAINWINDOW_H

#include <QMainWindow>
#include "Elevator.h"
#include "ElevatorWidget.h"
#include "FloorButtonsWidget.h"
#include "LogPanel.h"  
#include "Dispatcher.h" // ��ӵ�����ͷ�ļ�
class MainWindow : public QMainWindow
{
    Q_OBJECT

public:
    explicit MainWindow(QWidget* parent = nullptr);
    ~MainWindow();

private:
    Elevator* elevator1;                // ���ݶ���
    ElevatorWidget* elevatorWidget1;    // ������ʾ�ؼ�
    Elevator* elevator2;                // ���ݶ���
    ElevatorWidget* elevatorWidget2;    // ������ʾ�ؼ�
    Elevator* elevator3;                // ���ݶ���
    ElevatorWidget* elevatorWidget3;    // ������ʾ�ؼ�
    Elevator* elevator4;                // ���ݶ���
    ElevatorWidget* elevatorWidget4;    // ������ʾ�ؼ�
    Elevator* elevator5;                // ���ݶ���
    ElevatorWidget* elevatorWidget5;    // ������ʾ�ؼ�
    FloorButtonsWidget* buttonsWidget; // ¥�㰴ť�ؼ�
    LogPanel* logPanel;                //��־�ؼ�
    Dispatcher* dispatcher;  // ������
};

#endif // MAINWINDOW_H
