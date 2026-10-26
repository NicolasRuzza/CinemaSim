# Every scenario is tagged @ignore until the steps that make it pass exist.
# Remove the tag in the same pull request that implements the scenario.
Feature: Confirm a reservation
  As a customer at a self-service kiosk
  I want to confirm the payment of the seat I am holding
  So that the seat is mine for good

  Background:
    Given a session "S1" with the seats "A1" and "A2" available

  @ignore
  Scenario: Confirm the payment of a held seat
    Given customer "Bob" holds seat "A1" of session "S1"
    When customer "Bob" confirms the payment of his reservation
    Then the confirmation is accepted
    And the purchase of seat "A1" of session "S1" is completed

  @ignore
  Scenario: Accept a repeated confirmation by the same customer
    Given customer "Bob" holds seat "A1" of session "S1"
    And customer "Bob" confirms the payment of his reservation
    When customer "Bob" confirms the payment of his reservation
    Then the confirmation is accepted

  @ignore
  Scenario: Accept a confirmation when the purchase is already completed
    Given customer "Bob" holds seat "A1" of session "S1"
    And customer "Bob" confirms the payment of his reservation
    And the purchase of seat "A1" of session "S1" is completed
    When customer "Bob" confirms the payment of his reservation
    Then the confirmation is accepted
    And exactly 1 purchase of seat "A1" of session "S1" exists

  @concurrency @ignore
  Scenario: Simultaneous confirmations of the same reservation produce a single purchase
    Given customer "Bob" holds seat "A1" of session "S1"
    When customer "Bob" confirms the payment of his reservation 10 times at the same time
    Then all 10 confirmations are accepted
    And exactly 1 purchase of seat "A1" of session "S1" is completed

  @ignore
  Scenario: Reject a hold on a seat confirmed by another customer
    Given customer "Bob" holds seat "A1" of session "S1"
    And customer "Bob" confirms the payment of his reservation
    When customer "Jessica" holds seat "A1" of session "S1"
    Then the hold is rejected because the seat is already sold

  @ignore
  Scenario: Reject a confirmation of a nonexistent reservation
    When customer "Bob" confirms the payment of a nonexistent reservation
    Then the confirmation is rejected because the reservation does not exist

  @ignore
  Scenario: Reject a confirmation by a customer who does not own the reservation
    Given customer "Bob" holds seat "A1" of session "S1"
    When customer "Jessica" confirms the payment of the reservation of customer "Bob"
    Then the confirmation is rejected because the reservation belongs to another customer

  @ignore
  Scenario: A reservation can still be confirmed just before the hold expires
    Given customer "Bob" holds seat "A1" of session "S1"
    And 4 minutes and 59 seconds have passed
    When customer "Bob" confirms the payment of his reservation
    Then the confirmation is accepted

  @ignore
  Scenario: Reject a confirmation after the hold has expired
    Given customer "Bob" holds seat "A1" of session "S1"
    And 5 minutes have passed
    When customer "Bob" confirms the payment of his reservation
    Then the confirmation is rejected because the reservation does not exist

  @ignore
  Scenario: A confirmed seat stays unavailable beyond the original hold time
    Given customer "Bob" holds seat "A1" of session "S1"
    And 4 minutes have passed
    And customer "Bob" confirms the payment of his reservation
    And 9 minutes have passed
    When customer "Jessica" holds seat "A1" of session "S1"
    Then the hold is rejected because the seat is already sold

  @ignore
  Scenario: A confirmation that is never processed releases the seat after the confirmation window
    Given customer "Bob" holds seat "A1" of session "S1"
    And customer "Bob" confirms the payment of his reservation
    And no payment is being processed
    And 10 minutes have passed
    When customer "Jessica" holds seat "A1" of session "S1"
    Then the hold is accepted
